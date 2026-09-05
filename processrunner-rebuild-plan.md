# ProcessRunner rebuild — plan

Temporary working document. Delete once the work lands.

## Status

**Observables 0 to 6 are complete**, green on Windows and Linux, with no `[Platform(Exclude)]` markers left anywhere.

**Baseline**: 1,427 tests across 9 projects, green on Windows, E2E excluded. `FlacHasher.Win.Tests` is in the solution but holds no source files at all, so it contributes nothing to any gate.

**The Linux gate always exits 1.** `docker compose -f docker-compose-lnx.yml run --rm tests` filters out every E2E test, so the E2E assembly matches none and vstest returns 1 for "no test matches the given testcase filter". Judge that run by the absence of `Failed!` lines, not by its exit code. Worth a separate look: any CI step that trusts this exit code has been failing or is being ignored.

Settled along the way:

- A Ctrl+C'd child reports `130` on Unix, and the Windows-only constant meant Linux reported a cancellation as a decoder failure. Fixed; the test failed on Linux before and passes now.
- An executable that cannot be run throws `Win32Exception` synchronously, out of `RunAndReadOutput` itself, before any stream exists.
- `showProcessOutput: true` leaves `ProcessErrorOutput` null and `IsProcessOutputCaptured` false, and the child's stderr lands in the test runner's output. Harmless, but it is why a passing run is not silent.
- **Reap latency does not trip a healthy process at any sane timeout.** A child that lingers 0 ms or 300 ms after closing stdout is reaped well inside a 2 s wait, and production allows 1000 ms. The reap check only fires on a process that genuinely will not exit, so turning it into a failure costs nothing in false positives.
- Holding stdout open and refusing to exit are separate failures and report separately: no EOF is the timeout's business, EOF-then-hang is the reap check's.
- **The unbounded drain wait is gone by construction, not by timing.** The reader now fills a shared buffer instead of returning one, so a report takes a snapshot and never joins the task. There is nothing left to hang on, which is worth more than a timeout would have been: a cancellation token cannot interrupt a read already in progress, so the old `Cancel()` had nothing to break the wait with.
- The `IsProcessOutputCaptured: true` with null output contradiction is likewise structural now. Whatever the reader collected before it stopped is always available, so the flag means what it says: stderr was redirected.
- `ProcessRunnerFactory` in the application layer is the single place that converts a user's seconds and kibibytes into the runner's milliseconds and bytes. Both applications go through it.
- **Killing a real process unblocks a blocked read cleanly on Linux, as an end of stream.** Every cancellation and timeout test passes there against a real process. The `[Platform(Exclude = "Linux")]` markers were describing the anonymous-pipe fakes, not the platform: closing one of those under a blocked read raises `IOException`/EINTR, which a real child's exit does not. One marker is left, on a disposal test in `General.cs`, and belongs to observable 6.

- Nothing in the solution catches `TimeoutException`, so it can leave the contract without a single call-site change.
- Executable resolution holds under `UseArtifactsOutput`. The Linux image builds to `/src/build-output` with a layout unlike the Windows one, and the `FakeDecoderOutputDirectory` assembly attribute still points at the executable.
- Adding a setting is cheap: help text (`Help.cs`) and the Win settings form (`SettingsForm.BuildDynamicControls`) are both reflection-driven off the attributes, so one decorated property propagates everywhere.
- Removing a setting is silent: `ParameterReader.GetParameters` only walks the target class's properties, so an unknown INI key is never examined — no error, no warning, and the user loses the setting without being told.
- `ProcessRunner` has four construction sites: `FlacHasher.Cmd/Program.cs`, `FlacHasher.Win/Program.cs`, and both Utils apps.

Still open in observable 2 onwards. Observable 1 is done: `Output.cs` (7 tests) and `Exit.cs` (7 tests) cover expanded output fidelity, chunking, an empty source, streaming laziness, argument quoting, exit-code mapping, stderr capture and its absence, the Ctrl+C mapping on both platforms, and an unrunnable executable.

---

## Working agreement

- **Commit per observable**, in the established message style. **No pushing**, no interaction with PR #58.
- **Gate before every commit**: the full suite on **Windows and Linux**, excluding E2E. E2E is checked once at the end, if it can run locally at all.
- **Old tests are deleted as they are superseded**, in the same commit as their replacement, so the diff shows the swap and no window exists where two suites assert contradictory things about the same behaviour.
- **This document is kept current** as each observable lands.
- **Autonomy**: a finding that contradicts an assumption is decided, implemented and flagged in *Decisions taken alone*. The exception hierarchy, the precedence order and the public contract are not mine to change — a finding that would alter any of those stops for review.
- **FakeDecoder is not modified.** Anything it cannot produce is recorded in *FakeDecoder: capabilities not added*.
- **Delegated**: the ★ `startWaitMs` reproduction, and observable 7. Everything else stays in the main thread: a test that asserts the wrong thing fails by passing, and reviewing for that costs as much as writing it.

---

## Decisions

### Exception design

`ExecutionException` gains a `protected` constructor taking message + stderr + `IsProcessOutputCaptured`, so derived types supply their own text.

| Situation | Type | `ExitCode` |
|---|---|---|
| Caller cancels | `OperationCanceledException` — outside the hierarchy | — |
| Consumer disposes the stream | nothing thrown | — |
| Timed out, killed before EOF (**output unusable**) | `ProcessTimeoutException : ExecutionException` | `0` |
| EOF reached, child won't reap (**output complete**) | `ProcessNotRespondingException : ExecutionException` | `0` |
| Child exited `0` with input left undelivered | `PrematureExitException : ExecutionException` | the child's real `0` |

The BCL `TimeoutException` is not part of the contract.

Cancellation stays an `OperationCanceledException` because `DecoderStream` and `MultiFileHasher` both place `catch (OperationCanceledException) { throw; }` ahead of everything else.

`PrematureExitException` fires **only** when the exit code is `0`. A child that decided it had all the input it needed is not good enough: we know it did not read what we meant it to.

### Invariant

After a kill, the exit code belongs to the OS. It is never reported as the program's exit code and never compared against `ExitCode_CtrlC`.

### Precedence

cancellation → timeout → won't-reap → non-zero exit → stdin-not-delivered.

Stdin failure surfaces only when the run is otherwise clean: every kill breaks that pipe, so without the rule a stdin error races every cancellation. A child that exits non-zero *and* left input undelivered reports the plain `ExecutionException`.

### Ctrl+C exit codes

The check gets a Unix counterpart: `0xC000013A` (−1073741510) on Windows, `130` on Unix, platform-conditional. Without it a Ctrl+C on Linux tells the user their file is corrupt. It stays a backstop — `Console.CancelKeyPress` cancels the token, so the cancellation path usually wins the race.

### Error output capture

- Retention is **bounded, keeping the tail**: errors land at the end, progress spam at the front. Draining continues past the cap, discarding oldest, so the child never blocks on a full pipe.
- New setting **`DecoderInfoOutputMaxSizeKb`**, INI-only, default **64**. `-1` means no limit. `0` means use the default. Any other negative is rejected as invalid.
- The library takes **bytes** (`maxErrorOutputBytes`); KiB is a human convenience that belongs with the rest of the configuration.
- The drain timeout keeps deriving from `exitTimeoutMs`, but the wait becomes **bounded** — the unbounded `GetAwaiter().GetResult()` after `Cancel()` goes.
- On a partial capture, `IsProcessOutputCaptured` stays **`true`**. The flag answers "was stderr redirected at all", which is what the CLI branches on, and truncated output beats none.

### Constructor

```csharp
public ProcessRunner(
    int timeoutMs,          // was timeoutSec
    int exitTimeoutMs,
    int startWaitMs,
    int maxErrorOutputBytes,
    bool showProcessOutput)
```

`ProcessTimeoutSec` stays in seconds as the user-facing setting; a static helper does seconds→milliseconds, so the conversion is written once for all four construction sites.

**Hazard**: four consecutive `int` parameters, and `timeoutMs` changes units without changing type. An un-updated call site still compiles and silently means something 1000× smaller. All four are in this repo; nothing guards a fifth.

### An abandoned stream is already covered

A consumer that reads part of the output and neither reaches EOF nor disposes does **not** leak. `outputReadTask` never completes, so the timeout wait expires, kills the process and disposes it in the `finally` — 180 s at the production default. The `TimeoutException` lands on a task nobody observes and is swallowed, which does not matter: the kill and the dispose both happen.

The only gap is `timeoutSec: -1`, which forgoes the safety net by definition, exactly as it does for a decoder that genuinely hangs. Documentation only; no code change, no test.

### Testing

- Tests drive the two `RunAndReadOutput` overloads. `--expand` by default; plain output only where the stdout pipe buffer is load-bearing.
- `GetOutputStream_WaitProcessExitInParallel` becomes `internal` with `[assembly: InternalsVisibleTo("ExternalProcess.Tests")]`. The retained fake-process tests need it — it is the only injection point for `IExternalProcess` — and the shipped surface becomes exactly the two `RunAndReadOutput` overloads.
- **Sequential use only.** Nothing in the app runs two decoders at once, so observable 7 asserts that repeated sequential runs stay clean and claims no concurrency contract.
- Budget: under ~60 s on Windows. Deliberate waits stay in the low hundreds of milliseconds except where semantics demand seconds. `[NonParallelizable]` comes off wherever tests do not contend for a shared resource.

---

## Order of work

The unit of work is an **observable** — one piece of behaviour a caller can see — numbered below. There are no phases, and no separate probing stage: tests and fixes advance together within an observable, so that no test asserts behaviour a later fix redefines, and each probe sits in the observable whose design decision it settles. A probe that answers its question is committed as a test rather than recorded and discarded.

Refer to work by observable number. "Observable 2" is the reap/teardown block, and so on.

| # | Observable | Contains | Parallel |
|---|---|---|---|
| 0 | **Harness** | see below | serial |
| 1 | **Output, exit codes, arguments** | expanded output fidelity, streaming laziness, `ArgumentList` quoting (a two-word `--progress-message`), exit-code mapping, a missing or unrunnable executable (`Start()` throws synchronously, before any stream exists), and `ExitCode_CtrlC` gaining its Unix counterpart. Probe: `ExitCode_CtrlC` on Unix | hand off |
| 2 | **Reap / teardown** | `ProcessNotRespondingException`, the kill-code invariant, the `Program.cs` branch. Probe: real reap latency (`--linger`) | serial |
| 3 | **Stderr lifetime** | the bounded tail buffer and `DecoderInfoOutputMaxSizeKb`, the bounded drain wait, `IOException` on Linux, disposal on every path, the contradictory `IsProcessOutputCaptured: true` with null output. **Carries the constructor change** — `maxErrorOutputBytes` arrives here, so `timeoutSec`→`timeoutMs` and the four call sites land in the same commit. Probe: the unbounded wait after `Cancel()` | alongside 2 |
| 4 | **Cancellation + timeout** | `ProcessTimeoutException`, `Kill` guard symmetry. Probes: does killing unblock a blocked read, and as `0` or `IOException` — what the `[Platform(Exclude)]` markers hide; and whether `process.Dispose()` on this path leaves an in-flight consumer read working, which the whole design rests on | after 2 |
| 5 | **Stdin delivery** | `PrematureExitException`, under the precedence rule; the runner's ownership of the caller's input stream, which it disposes. Probes: broken pipe via `--finish-after-reads`, >64 KiB, **no expansion**; and mutual stdin/stdout block — a child emitting far more than it consumes (`--expand`) against a slow or absent consumer | after 4 |
| 6 | **Stream + handle contract** | `TrySetResult` (a successful read racing `Close` currently reports a cancellation), and releasing the stdout stream on close. An abandoned stream is *not* in scope — see the decision above | any time |
| 7 | **Repeat runs** | the net under everything above, and the only way the handle leak is observable. Sequential only | last, delegated |
| ★ | **`startWaitMs`** | default 100 ms, blocking the *caller's* thread every run — 30 s of pure sleep on a 300-track batch. **Attempt to reproduce** the failure it guards against, at `0`, over many iterations, both platforms. **No code change either way**: reproduced or not, the finding is recorded and we revisit it together. The original problem was real — something about the child's stdin not being open yet | early, independent, delegated |

`startWaitMs` sits outside the sequence: it depends on nothing else and is plausibly the largest user-visible win here.

---

## Observable 0 — harness

`FakeDecoder.Tests/Setup` already has a working `TestEnvironment`, `TestPayload` and the `ResolveFakeDecoderOutputDirectory` target; `Flags.cs` is the starting point for the arg builder.

- Copy the MSBuild target and `TestEnvironment` verbatim. No CliWrap — the runner under test is the launcher.
- Fluent arg builder that refuses combinations FakeDecoder rejects, so a mis-built probe fails loudly instead of arriving as exit-code-1-with-empty-stdout.
- `Expected(source, expand, xor)` = `source.SelectMany(b => Repeat((byte)(b ^ xor), n))`.
- Per-test timeouts, so a hang kills its own test rather than the run.
- A `--filter`ed Linux invocation: `docker-compose-lnx.yml` runs the whole solution, which is too slow for the inner loop.

  ```
  docker compose -f docker-compose-lnx.yml run --rm --entrypoint bash tests -c 'dotnet test ExternalProcess/ExternalProcess.Tests/ExternalProcess.Tests.csproj -c Release --nologo -v q /p:EnableWindowsTargeting=true --filter "FullyQualifiedName~ProcessRunner_Tests" 2>&1 | tail -6'
  ```
- Confirm nothing catches `TimeoutException` before it leaves the contract.
- `ProcessStartInfoFactory` sets `CreateNoWindow = false`, so children share the test runner's console, and under `showProcessOutput: true` their stderr lands in the test output. Establish what that looks like before it gets mistaken for a failure.

### Constraints

- Exact byte counts require `--file`; a pipe read returns whatever has arrived, so `--finish-after-reads` over `--stdin` pins down no length.
- `read-chunk-size × (expand + 1) ≤ 64 MiB`.
- Sourceless runs accept only `--linger`, `--exit-code` and the exit messages.

### Three stall shapes, kept distinct

| Flag | Consumer state |
|---|---|
| `--write-delay -1` | blocked mid-stream, output incomplete, stdout open |
| `--keep-stdout-open -1` | all output delivered, no EOF |
| `--linger -1` | EOF delivered, process alive — observable 2's shape |

---

## Deliberately not probed

- **fd 2 landing on stdout's pipe.** `RedirectStandardOutput` is always true, so the child's fd 1 is a dedicated pipe and its fd 2 is either its own pipe or the parent's inherited one. The FakeDecoder README's `2>&1` warning does not apply here.
- **Child dying at startup on an invalid standard handle.** `ProcessStartInfo` offers no way to hand a child an invalid standard handle, and a test host's fd 2 is always valid. Remains a documented `FlacHasher.Win` caveat; the behaviour is recorded in the FakeDecoder README.
- **Whether an undrained stderr stalls stdout.** The runner always drains stderr when it redirects it, so this cannot be observed through the code under test and no decision depends on it.
- **`exitTimeoutMs: 0`.** Production defaults to 1000 ms (`ApplicationSettings.cs`, `settings.ini`). The live defect is the misreported kill code, which occurs at 1000 ms too.

---

## Known, and left alone

`ProcessOutputStream.Read` throws `InvalidOperationException` past EOF and `Flush()` throws outright, both departures from the `Stream` contract. Neither is reachable in this codebase: `HashAlgorithm.ComputeHash(Stream)` neither flushes nor reads past EOF, and `DecoderStream` throws from `Flush` too, so it is house style rather than an oversight. Recorded so it is not rediscovered as a bug.

---

## Test double inventory

**Retained** — for what a real process physically cannot do (a child refusing to die, stderr reads throwing):
`ExternalProcessFake`, `StdoutStream` (used by the fake), `ThrowingReadStream`.

**Removed:**
`ExternalProcessPiped`, `EndlessFakeReadStream`, `SignalWaitingMemoryStream`, `DelayingMemoryStream`, `ReadSignallingMemoryStream`, `FakeWriteStream`.

---

## Downstream

`DecoderStream` catches `ExecutionException`, so both new types wrap into `DecoderException` unchanged.

- `Program.cs:136` prints "Decoder returned code {ExitCode}", which would read "code 0". Needs its own branch.
- `Verification.cs:90` and `FormX.cs:601` render `Message` and nothing else, so both new messages must stand alone without surrounding context. That is what the protected constructor is for.
- Both types reach `MultiFileHasher` as `Audio.IOException`, so under `continueOnError` the batch carries on per file. Likely right for one stuck track — confirm rather than inherit.

---

## Definition of done

- Green on Windows and under `docker-compose-lnx.yml`, with no `[Platform(Exclude)]` anywhere.
- Every process-behaviour assertion driven by a real FakeDecoder process.
- `Unit/` limited to what a real process physically cannot do.
- `GetOutputStream_WaitProcessExitInParallel` demoted to `internal`.

---

# For review

Three sections below collect everything deliberately deferred. Nothing here is settled; all of it is waiting on a read-through once the work is done.

## Decisions taken alone

Findings that contradicted an assumption, and what was done about them.

### Letting go of the process' pipe interrupts a read in flight, on Unix only

**Expected**: releasing the stdout stream on close would be housekeeping, since nothing else lets go of the process' end of the pipe — `Process.Close` deliberately leaves its stream readers alone.

**Measured**: on Linux, disposing of that stream while a read is outstanding interrupts the system call: `IOException: Interrupted system call` wrapping `SocketException (4)`, out of `PipeStream.ReadCore`. Windows reports the stream as disposed of instead. The same difference is what the old `[Platform(Exclude = "Linux")]` markers were describing, except those blamed the platform while testing anonymous-pipe fakes; this is the real thing, and it only appeared once the stream was genuinely released.

**Decided**: `ProcessOutputStream` notes that it is closing before it lets go, and a read torn down in that window comes back as an end of stream rather than a fault. The cancellation is then reported from the usual place. The caller asked for the close, so an I/O error is not the honest answer.

**If reversed**: the alternative is not releasing the pipe at all, which is what leaked a handle per file hashed. Reporting the `IOException` as-is would be worse still — disposal would be quiet on Windows and an error on Linux.

<!-- Each entry: what was expected, what the probe actually showed, what was decided, and what would change if the decision were reversed. -->

## Exception message wording

`FormX.ReportExecutionError` and `Verification.cs` display nothing but `Message`, so for those users the message is the entire explanation. The three new messages are written to stand alone and are listed here to be adjusted to taste.

**`ProcessNotRespondingException`**
> The process stopped responding after writing all of its output and had to be terminated, so there is no knowing whether it finished the job. Process error output\n: {output}

… or, when stderr was not redirected, `Process error output has not been captured`.

Seen by: the Win app's status box, the CLI's verification listing. The CLI's hashing path prints its own wording instead:

> Couldn't Decode audio. The decoder produced its output but then stopped responding and had to be terminated, so there's no telling whether it finished the job.
> Possible reasons: the decoder is waiting on something, or is misconfigured/given incorrect parameters.

- A process that produces far more than it consumes does not deadlock against the feeding: both pipes are in play at once and neither wedges the other, at a 4x expansion over a 256 KiB input.

**`ProcessTimeoutException`**

> The process took longer than it is allowed and was terminated before it finished, so its output is incomplete. Process error output\n: {output}

… or, when stderr was not redirected, `Process error output has not been captured`.

Seen by: everywhere a decoder failure is reported. It replaces the bare `TimeoutException`, which carried no process output at all.

**`PrematureExitException`**

> The process stopped reading its input before all of it had been sent, and then exited with code {code}. Only part of the data reached it, so whatever it produced covers only that part. Process error output\n: {output}

… or, when stderr was not redirected, `Process error output has not been captured`.

Seen by: everywhere a decoder failure is reported. Unlike the two above, this one carries the code the process really did choose for itself, which is always `0` — a code of its own outranks it.

## FakeDecoder: capabilities not added

Behaviours a test wanted that the stub cannot currently produce, recorded rather than built, to be revisited together. Empty so far.

<!-- Each entry: the behaviour, the test that wanted it, and how the test was written instead. -->

### Known already

- **A child that refuses to die.** SIGKILL cannot be refused and `TerminateProcess` cannot be declined, so no real program can produce this. Stays a fake-process test in `Unit/`.
- **Stderr reads that throw.** A property of the reading code, not of the child. Stays a fake-process test in `Unit/`.
- **A genuine Ctrl+C.** The stub cannot raise one, and Unix truncates exit status to a byte so the Windows constant is unreachable there. Exercised through `--exit-code` on Windows, and as a fake-process test for the mapping itself.
