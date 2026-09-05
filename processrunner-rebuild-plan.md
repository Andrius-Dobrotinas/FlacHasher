# ProcessRunner rebuild — plan

Temporary working document. Delete once the work lands.

## Status

**Observable 0 (harness) is complete**, green on Windows and Linux: 6 tests in `ProcessRunner_Tests/Output.cs` driving a real FakeDecoder process through `RunAndReadOutput`.

Settled along the way:

- Nothing in the solution catches `TimeoutException`, so it can leave the contract without a single call-site change.
- Executable resolution holds under `UseArtifactsOutput`. The Linux image builds to `/src/build-output` with a layout unlike the Windows one, and the `FakeDecoderOutputDirectory` assembly attribute still points at the executable.

Still open in observable 1, all of it on failure paths rather than the happy one: exit-code mapping, `ExitCode_CtrlC` on Unix, a missing or unrunnable executable, and what `showProcessOutput: true` with `CreateNoWindow = false` does under the test runner.

---

## Decisions

### Exception design

`ExecutionException` gains a `protected` constructor taking message + stderr + `IsProcessOutputCaptured`, so derived types supply their own text.

| Situation | Type |
|---|---|
| Caller cancels | `OperationCanceledException` — outside the hierarchy |
| Consumer disposes the stream | nothing thrown |
| Timed out, killed before EOF (**output unusable**) | `ProcessTimeoutException : ExecutionException` |
| EOF reached, child won't reap (**output complete**) | `ProcessNotRespondingException : ExecutionException` |

Both report `ExitCode = 0`. The BCL `TimeoutException` is not part of the contract.

Cancellation stays an `OperationCanceledException` because `DecoderStream` and `MultiFileHasher` both place `catch (OperationCanceledException) { throw; }` ahead of everything else.

### Invariant

After a kill, the exit code belongs to the OS. It is never reported as the program's exit code and never compared against `ExitCode_CtrlC`.

### Precedence

cancellation → timeout → won't-reap → non-zero exit → stdin-not-delivered.

Stdin failure surfaces only when the run is otherwise clean: every kill breaks that pipe, so without the rule a stdin error races every cancellation.

### Testing

Tests drive the two `RunAndReadOutput` overloads. `--expand` by default; plain output only where the stdout pipe buffer is load-bearing.

---

## Order of work

The unit of work is an **observable** — one piece of behaviour a caller can see — numbered below. There are no phases, and no separate probing stage: tests and fixes advance together within an observable, so that no test asserts behaviour a later fix redefines, and each probe sits in the observable whose design decision it settles. A probe that answers its question is committed as a test rather than recorded and discarded.

Refer to work by observable number. "Observable 2" is the reap/teardown block, and so on.

| # | Observable | Contains | Parallel |
|---|---|---|---|
| 0 | **Harness** | see below | serial |
| 1 | **Output, exit codes, arguments** | expanded output fidelity, streaming laziness, `ArgumentList` quoting (a two-word `--progress-message`), exit-code mapping, a missing or unrunnable executable (`Start()` throws synchronously, before any stream exists). Gives `ExitCode_CtrlC` a Unix counterpart or an explicitly Windows-only scope. No fixes expected elsewhere — proves the harness before anything trusts it. Probe: `ExitCode_CtrlC` on Unix | hand off |
| 2 | **Reap / teardown** | `ProcessNotRespondingException`, the kill-code invariant, the `Program.cs` branch. Probe: real reap latency (`--linger`) | serial |
| 3 | **Stderr lifetime** | bounded harvest with its own timeout, a bound on how much stderr is buffered (today the whole run's progress output accumulates in a `MemoryStream`), `IOException` on Linux, disposal on every path, the contradictory `IsProcessOutputCaptured: true` with null output. Probe: the unbounded wait after `Cancel()` | alongside 2 |
| 4 | **Cancellation + timeout** | `ProcessTimeoutException`, `Kill` guard symmetry. Probes: does killing unblock a blocked read, and as `0` or `IOException` — what the `[Platform(Exclude)]` markers hide; and whether `process.Dispose()` on this path leaves an in-flight consumer read working, which the whole design rests on | after 2 |
| 5 | **Stdin delivery** | delivery failure as an error, under the precedence rule; the runner's ownership of the caller's input stream, which it disposes. Probes: broken pipe via `--finish-after-reads`, >64 KiB, **no expansion**; and mutual stdin/stdout block — a child emitting far more than it consumes (`--expand`) against a slow or absent consumer | after 4 |
| 6 | **Stream + handle contract** | `TrySetResult` (a successful read racing `Close` currently reports a cancellation), release the stdout stream on close, and a consumer that abandons the stream without reaching EOF or disposing — today nothing kills or disposes the process | any time |
| 7 | **Repeat + concurrent runs** | the net under everything above, and the only way the handle leak is observable | last |
| ★ | **`startWaitMs`** | default 100 ms, blocking the *caller's* thread every run — 30 s of pure sleep on a 300-track batch. Probe whether it is needed at all, or only on the stdin path | early, independent |

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
