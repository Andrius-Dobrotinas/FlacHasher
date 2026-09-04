# FakeDecoder

A fake external program for tests. It reads bytes from a source, writes them to stdout, prints progress to stderr, and can be told to fail, stall or linger after finishing the job. Everything it does is driven by command-line flags, so a test can reproduce whatever process behaviour it needs to assert on.

## Flags

```
--file <path>              read bytes from this file
--stdin                    read bytes from standard input
--xor <hex-byte>           XOR every byte read with this value before writing it out
--expand <n>               write every byte read n times over, so the output outgrows the source
--read-chunk-size <n>      bytes read from the source per read; a write is this many times --expand (default 4096)
--output-chunk-delay <ms>  pause before each stdout write; -1 = wait forever
--finish-after-reads <n>   leave the rest of the source unread and finish the run after n reads
--progress-message <text>  written to stderr after each chunk
--success-message <text>   written to stderr before exit, when the exit code is 0
--error-message <text>     written to stderr before exit, when the exit code is non-zero
--keep-stdout-open <ms>    wait with stdout still open; -1 = wait forever
--linger <ms>              wait after closing stdout; -1 = wait forever
--exit-code <n>            exit code to return (default 0)
```

At most one source may be given, and each flag may be given at most once.
An unrecognised flag, a repeated flag, a missing, unparseable or out-of-range value, a `--file` that doesn't exist, two sources, a read chunk that multiplied by the expansion exceeds 64 MiB, or `--keep-stdout-open` without a source are all usage errors: the flag list goes to stderr, stdout is left empty, and the exit code is 1. Keeping the help off stdout is what stops it from leaking into byte-for-byte assertions.

The ranges: `--read-chunk-size` and `--finish-after-reads` are 1 or more; `--expand` is 2 or more; `--output-chunk-delay`, `--keep-stdout-open` and `--linger` are -1 or more; `--exit-code` is any integer; `--xor` is a hexadecimal number that fits in a byte, written without a `0x` prefix.

Apart from that one usage-error case, it never invents an exit code - whatever `--exit-code` says is what it returns. The buffers are sized from the arguments and allocated before a byte is read, which is why a read chunk times an expansion is capped: an allocation that throws would cost the run its exit code, and a cap turns that into an ordinary usage error instead. Unix keeps only the low byte of an exit status, so a negative code arrives at the caller wrapped round (-1 comes back as 255); Windows hands it over verbatim.

## What it does, in order

### Given a source

1. write the source to stdout in chunks, expanded and XORed if asked, with `--progress-message` after each chunk
2. wait `--keep-stdout-open` with stdout still open, so the consumer gets no EOF yet
3. close stdout - this is what hands the consumer its EOF
4. wait `--linger`
5. write `--success-message` or `--error-message`, whichever the exit code selects
6. exit with `--exit-code`

Steps 2 and 4 are both waits, but on opposite sides of the EOF, and they exercise different behaviour in the code under test: a consumer blocked waiting for output, versus a process that has finished writing but won't quit.

### Given no source

1. write `Nothing to process` to stderr, followed by the flag list if there were no arguments at all
2. wait `--linger`
3. write `--success-message` or `--error-message`, whichever the exit code selects
4. exit with `--exit-code`

Stdout is never opened, so the consumer's EOF arrives with the exit rather than ahead of it. That is why `--keep-stdout-open` is a usage error without a source: there would be no write to hold off, and the wait would pass unnoticed.

## Features worth noting

### Transforming input into a longer output

To simulate a compressed audio decoder, output must be longer than input.
`--expand` makes the output outgrow the source: every byte read is written n times over. Which bytes get invented doesn't matter to anything - what matters is that the output is a constant multiple of the input, whatever the chunking, so a test can spell out what it expects with `bytes.SelectMany(x => Enumerable.Repeat(x, n))`.
`--xor` still applies to everything on its way out, duplicates included.

The reads are unaffected by expansion: `--read-chunk-size` sizes the read, and a write is simply that many bytes times n.

### Stopping before the source runs out

Writing stops early in two cases: `--finish-after-reads` is reached, or the consumer lets go of the read end and the pipe breaks. Neither counts as a failure - the run carries on to the exit routine and returns the `--exit-code` it was given, the same as any other run. The consumer's EOF arrives on schedule too: stdout is closed at step 3 as always, so an early finish looks to it exactly like the source running out.

Either way the source is left partly read. With `--stdin` that means whoever is writing to it is left with a reader that's gone, and their next write breaks - exactly what `head` does to the command feeding it in a shell pipeline. The remainder is deliberately not drained first: being able to produce that broken pipe is the point, and draining would hide it.

### Signaling EOF while staying alive
The application needs to be able to **Signal end-of-output while staying alive** to fake a teardown latency situation. EOF and observable process exit aren't simultaneous: the pipe closes as part of the process tearing down; the process record isn't reaped until slightly after. So a WaitForExit with a short timeout can return false for a process that is for all intents finished.

Implementation of this is more involved than it looks. A consumer sees EOF only once every write end of the pipe has been closed, and normally that happens when the process exits - which is the very thing this has to avoid. Neither disposing of the stream `Console.OpenStandardOutput()` hands out nor, on Linux, releasing descriptor 1 is enough on its own. Only Windows and Linux are covered; macOS would need a different technique and none is implemented.

Getting it wrong doesn't fail loudly - it quietly collapses steps 2 and 4 of the sourced flow into the same thing. The reasoning is in the comments on `OpenStdout` and `CloseStdout` in [Program.cs](Program.cs); read them before changing any of it.
