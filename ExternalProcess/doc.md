# ProcessRunner

How the runner behaves and assumptions the implementation is based on.

---

## Facts about the environment

This is the observed behavior of the environment (Windows and Linux) -- the implementation is built on this.

- **A process that dies closes its end of its pipes**. A read already blocked on its output then ends at `0` — an ordinary end of stream, on both platforms. The timeout and cancellation paths rely on this after killing a process.
- **Closing my own end of a pipe while a read is in flight (or attempting to read after) - results differ by platform**. Unix interrupts the system call — `IOException: Interrupted system call` wrapping `SocketException (4)`, out of `PipeStream.ReadCore`; Windows throws `ObjectDisposedException`.
- **Disposing of the Process (`Process.Close`) does not dispose of the standard stream readers and writers it created** — it drops its references to them, and their handles stay open until somebody disposes of them directly. This is deliberate: it lets a consumer holding one of them go on reading after the process object itself is gone.
- **A Ctrl+C'd process reports `0xC000013A` (−1073741510) on Windows and `130` on Unix** — SIGINT, reported as 128 + the signal number.
- **OS buffers writes to the process' pipe** — a successful write does not mean the process has read it. This matters most for the last chunk of a transfer: a chunk smaller than the buffer can be written in full to a process that never reads a byte of it, and the write still succeeds.
- **The pipe buffer belongs to the operating system, not to the child, and its size cannot be set**: .NET passes `0` to `CreatePipe` on Windows, and there is no knob on Unix.

### Measurements

Taken on **Windows 11 Enterprise, build 10.0.26200**, and in the `mcr.microsoft.com/dotnet/sdk:8.0` container (**Debian 12 bookworm**, .NET SDK 8.0.424) under Docker Desktop, so on the **WSL2 kernel, 6.6.87.2-microsoft-standard-WSL2** — not a native Linux one. Magnitudes to reason with, not guarantees.

#### How much fits in a pipe before a write blocks

Bytes written to a child that never reads a single one of them, before the write blocks:

| | buffer |
|---|---|
| Windows | **4 KiB** (4096 bytes) |
| Linux (container) | **64 KiB** (65536 bytes) |

Bytes sitting in that buffer have been written but not read, and the writer cannot tell the difference.

#### How long a child takes to start and die

A do-nothing .NET child, timed from `Process.Start` returning to the process being gone, 30 runs each:

| | min | median | max |
|---|---|---|---|
| Windows | 210 ms | **260 ms** | 298 ms |
| Linux (container) | 102 ms | **119 ms** | 147 ms |

This is what makes a 100 ms delay land on different sides of the same race on the two platforms. It is not a difference in how the platforms treat pipes.

---

## Process Runner Behavior

- **If the decoder can't be started, `RunAndReadOutput` throws before it returns anything.** It starts the process on the calling thread and catches only that failure: a missing or unrunnable executable comes back as a `LaunchException` (message and `InnerException` taken straight from the `Win32Exception`, `IsProcessOutputCaptured` always `false`) — not through the returned stream, because there isn't one yet.
- `showProcessOutput: true` leaves `ProcessErrorOutput` null and `IsProcessOutputCaptured` false, and the child's stderr lands in the console it shares with its parent (my application).
- **`exitTimeoutMs` (1000 ms default) only starts counting once the process has closed its standard output.** It is an allowance for winding down after the work is already done, in case a process stops responding once it has closed stdout. The same value is used for stderr reading, but it's a separate allowance. Both std-err reading and process exit get the same allowance each. `Teardown.cs` tests this.
- **Holding stdout open and refusing to exit are separate failures and throw separately**: no EOF is the timeout's business, EOF-then-hang is the exit timeout's.
- **If reading the process' error output fails, that doesn't derail the exception about the process itself.** You still get that exception, carrying whatever error text was collected before the reading fell over — best effort, not all or nothing. `When_ReadingStdErr_Throws_PartWayThrough__Must_Report_WhatCameThrough_BeforeThat` pins it.
- `ExecutionException.IsProcessOutputCaptured: true` can never arrive with null output: whatever the reader collected before it stopped is always available, so the flag means what it says — stderr was redirected.
- **Feeding the process and reading its output happen at the same time, so a decoder that emits far more than it takes in can't deadlock the run.** The trap it avoids: the process fills its output pipe and blocks writing, so it stops reading its input, so the write to it blocks as well, and both sides wait for each other forever. Writing to stdin runs on its own background task while the caller reads the returned stream, so no pipe is left unattended.
- **Abandoning the reading**: results in a leak (unclosed stdout handle) only when `timeoutSec: -1`; otherwise, the process gets killed after `timeoutSec`.
- `startWaitMs` applies **only when the process is being fed through its standard input**, which covers draining its error stream too, since that starts in the same place. A process with nothing to receive has nothing to be too early for, so the file-based decoding path waits not at all rather than paying 100 ms (default) a track.

### Error output capturing

- Retention is **bounded, keeping the tail**: errors land at the end, progress spam at the front. Draining continues past the cap, discarding oldest, so the child never blocks on a full pipe.
- Setting **`DecoderInfoOutputMaxSizeKb`** controls the lenght of captured std-err (64 kb default). `-1` means no limit. `0` means use the default. Any other negative is rejected as invalid.
- **Before throwing, the runner gives the error output reader a moment to finish, but not forever** — it waits up to `exitTimeoutMs` (the same setting as the exit allowance; there is no separate one), then takes whatever is in the buffer and carries on. A stderr read that never returns cannot hang the caller.
- On a partial capture, `IsProcessOutputCaptured` stays **`true`**. The flag answers "was stderr redirected at all", which is what the CLI branches on, and truncated output beats none.

---

## Contract
- `RunAndReadOutput` returns a `ProcessOutputStream` right away, without waiting for the process to write anything. Only a failure to start the process (`LaunchException`) is thrown from the call itself; everything else the run can end in surfaces later, out of reading that stream.
- Reading the returned stream is what drives the run: a `Read` call returning `0` is conclusive - a failing run throws out of that very call instead of returning `0`, so there is never a `0` a caller needs to double-check with another read. That check only happens once a `Read` call actually reaches end of stream; a caller that stops reading after the process has already finished sees nothing until it reads again, however long that takes.
- `Close`/`Dispose` on the stream is always quiet. If the run is still going, closing it cancels the run, waits for the process to actually be gone, and only then releases this end of the pipe. Safe to call more than once.
- `ProcessOutputStream` rethrows `ExecutionException` as `Audio.IOException`, with the original exception preserved.

---

## Known limitations and caveats
These are genuine limitations, not choices

### Standard input: delivery means written - no way to actually confirm

The caller knows it has written everything; from there the process is expected to play fairly and report an honest exit code. `PrematureExitException` is raised only when the process has exited **and** we know for a fact that the writing did not finish. That is the whole of the guarantee, and it is deliberate.

What it does not cover follows from the pipe buffer: anything smaller than it can be written in full to a child that never reads a byte, and the write succeeds. The same goes for the tail of any transfer — the last few bytes go into the buffer and the write returns whether or not they are ever read. A write is never an acknowledgement; only the child's exit code says it was satisfied, which is why a non-zero one outranks this.

**The tail is a blind spot, and file size does not close it.** What decides whether a failure to deliver is noticed is how much is left **unread** when the process gives up, not how much was sent. A process that abandons its input early leaves more than a buffer's worth outstanding, so the write blocks and then breaks, and it is caught — which is what `Feeding.cs` covers. A process that stops within the last 4 KiB (Windows) or 64 KiB (Linux) leaves a remainder that fits in the buffer: the write completes, delivery is marked, and if the process then exits `0` the run passes as clean. That holds for a 30 MB file exactly as it does for a 3 KB one.

Nothing in a pipe can close that. Only the process saying so can, and this one's way of saying so is its exit code.

**The size of the chunks fed in is not the lever it looks like.** `WriteToStdInAndDisposeOf` uses `CopyTo` without a buffer size, so it gets `Stream.DefaultCopyBufferSize` — 81,920 bytes, the largest multiple of 4096 still under the 85,000-byte Large Object Heap threshold, so the short-lived array dies in gen 0 rather than accumulating on the LOH. `CopyTo(destination, bufferSize)` would change it, and it is deliberately not used:

- A write completes once every byte of it has been taken into the pipe, so what decides whether the last one slips through unread is **the pipe's free space**, not the size of the chunk offered to it. Smaller chunks mean more writes, each of which can still fit, and the pipe is the size it is either way.
- 80 KiB is larger than either pipe buffer, so **every write except the last blocks against a process that isn't draining**. That is what makes abandonment detectable at all.
- It also sets progress granularity: `ProgressReportingReadOnlyFileStream` raises its event once per `Read`, so a 30 MB track reports about 380 times. At 4 KiB it would report 7,500 times.

An input smaller than the pipe buffer is a single write, and that write is the last one, so the whole transfer can complete unread.

### What couldn't be covered with tests

- **The three pipe releases** — standard output, standard error and standard input. The only signal that separates them from their absence is Linux-only: 2 descriptors added against 25, where Windows read 31 handles against 40 and could not tell them apart. Reasoning in *Releasing the process' pipes is measurable on Linux and not on Windows*.
- **The stderr reader's cancellation source, disposed on every path out.** Its whole effect is a handle no longer being held, which is the thing that cannot be counted portably.

#### Releasing the process' pipes is measurable on Linux and not on Windows

**Windows**, 30 runs, full assembly: with everything released, 31 handles added; with the stdout release taken out, 40. The two are indistinguishable. `Process.HandleCount` counts every kernel handle, and each run's `LongRunning` background tasks are real threads whose handles land in that number and are released on their own schedule — roughly a run's worth of drift on top of the signal. In isolation the same test read 241, so the number depends more on what ran before it than on the code under test.

**Linux**, same 30 runs, counting `/proc/self/fd`: **2 descriptors added with everything released, 25 with the stdout release taken out**. That separates cleanly. Descriptors are a narrower count than handles — threads do not appear among them — so the noise that swamped the Windows measurement is simply absent.

Both platforms share the masking that made the Windows numbers useless: an undisposed stream is finalizable, so a collection landing inside the window reclaims exactly what is being counted. Linux shows 25 rather than the 90-odd three pipes per run would amount to, which is that masking at work. It is enough of a margin; on Windows it was not.

A deterministic substitute does not hold up either: `CanRead` on the returned stream goes false after disposal on Windows and stays true on Linux.

So no test asserts the releases. The only signal that separates them from their absence works on Linux alone, and a guard that covers one platform and not the other is not worth having: the work is done on Windows, so that is where a regression would be introduced, and that is exactly where such a test would say nothing.