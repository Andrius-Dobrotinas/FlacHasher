# FakeDecoder

A fake external program for tests. It reads bytes from a source, writes them to stdout, prints progress to stderr, and can be told to fail, stall or linger after finishing the job. Everything it does is driven by command-line flags, so a test can reproduce whatever process behaviour it needs to assert on.

## Flags

The list below is for orientation. The program's own help is the copy that can't go stale - it carries every default, range and limit, and it's generated from the same constants the parsing enforces:

```
dotnet run --project tools/FakeDecoder
```

```
--file <path>              read bytes from this file
--stdin                    read bytes from standard input
--xor <hex-byte>           XOR every byte read with this value before writing it out
--expand <n>               write every byte read n times over, so the output outgrows the source
--read-chunk-size <n>      bytes to read from the source per read; a write is this many times --expand
--write-delay <ms>         pause before each stdout write; -1 = wait forever
--finish-after-reads <n>   leave the rest of the source unread and finish the run after n reads
--progress-message <text>  written to stderr after each write
--success-message <text>   written to stderr before exit, when the exit code is 0
--error-message <text>     written to stderr before exit, when the exit code is non-zero
--keep-stdout-open <ms>    wait with stdout still open; -1 = wait forever
--linger <ms>              wait after closing stdout; -1 = wait forever
--exit-code <n>            exit code to return (default 0)
```

At most one source may be given, and each flag may be given at most once.
An unrecognised flag, a repeated flag, a missing, unparseable or out-of-range value, a value that is itself a flag name, a `--file` that can't be read, two sources, buffers adding up past the cap, or a flag that needs a source given without one are all usage errors: the flag list goes to stderr, stdout is left empty, and the exit code is 1. Keeping the help off stdout is what stops it from leaking into byte-for-byte assertions.

Every value has a range, and `--xor` is a hexadecimal number that fits in a byte, written without a `0x` prefix; the help above has the particulars. No value may begin with `--`: a flag name in a value's place is taken for a typo, on the same reasoning that a repeated flag is - a test has to run with the arguments it spells out and no others.

Apart from that one usage-error case, it never invents an exit code - whatever `--exit-code` says is what it returns. The buffers are sized from the arguments and allocated before a byte is read, which is why their total is capped: an allocation that throws would cost the run its exit code, and a cap turns that into an ordinary usage error instead. An expansion needs a second buffer alongside the read one, so `--read-chunk-size` on its own may use the whole allowance, while with `--expand n` the chunk and its expansion together - n plus one buffers' worth - have to fit. Unix keeps only the low byte of an exit status, so a negative code arrives at the caller wrapped round (-1 comes back as 255); Windows hands it over verbatim.

## What it does, in order

### Given a source

1. read the source and write it to stdout, expanded and XORed if asked, with `--progress-message` after each write
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

Stdout is never opened, so the consumer's EOF arrives with the exit rather than ahead of it. Only `--linger`, the two exit messages and `--exit-code` mean anything on this path, and giving any of the others is a usage error rather than a run that quietly ignores them: `--keep-stdout-open` would have no write to hold off, `--write-delay` no write to pause before, `--expand` and `--xor` no bytes to transform. A flag that can't take effect is far likelier to be a mistake than an intention, and a run that swallowed it would look just like the one that was asked for.

## Features worth noting

### Transforming input into a longer output

To simulate a compressed audio decoder, output must be longer than input.
`--expand` makes the output outgrow the source: every byte read is written n times over. Which bytes get invented doesn't matter to anything - what matters is that the output is a constant multiple of the input, whatever the chunking, so a test can spell out what it expects with `bytes.SelectMany(x => Enumerable.Repeat(x, n))`.
`--xor` still applies to everything on its way out, duplicates included.

The reads are unaffected by expansion: `--read-chunk-size` sizes the read, and a write is simply that many bytes times n.

### Stopping before the source runs out

Writing stops early in two cases: `--finish-after-reads` is reached, or the consumer lets go of the read end and the pipe breaks. Neither counts as a failure - the run carries on to the exit routine and returns the `--exit-code` it was given, the same as any other run. The consumer's EOF arrives on schedule too: stdout is closed at step 3 as always, so an early finish looks to it exactly like the source running out.

How many bytes n reads amount to depends on the source. A file hands over exactly `--read-chunk-size` bytes every time until it runs out, so with `--file` the count of reads pins the output down to the byte. A pipe hands over whatever has arrived by then, which can be less, so with `--stdin` the read size is only an upper bound and n reads are worth no fixed number of bytes: a producer that writes 5 bytes, pauses, and then writes the rest leaves the first read 5 bytes long whatever the read size says.

Either way the source is left partly read. With `--stdin` that means whoever is writing to it is left with a reader that's gone, and their next write breaks - exactly what `head` does to the command feeding it in a shell pipeline. The remainder is deliberately not drained first: being able to produce that broken pipe is the point, and draining would hide it.

### Signaling EOF while staying alive
The application needs to be able to **Signal end-of-output while staying alive** to fake a teardown latency situation. EOF and observable process exit aren't simultaneous: the pipe closes as part of the process tearing down; the process record isn't reaped until slightly after. So a WaitForExit with a short timeout can return false for a process that is for all intents finished.

Implementation of this is more involved than it looks. A consumer sees EOF only once every write end of the pipe has been closed, and normally that happens when the process exits - which is the very thing this has to avoid. Neither disposing of the stream `Console.OpenStandardOutput()` hands out nor, on Linux, releasing descriptor 1 is enough on its own. Only Windows and Linux are covered; macOS would need a different technique and none is implemented.

Getting it wrong doesn't fail loudly - it quietly collapses steps 2 and 4 of the sourced flow into the same thing. The reasoning is in the comments on `OpenStdout` and `CloseStdout` in [Program.cs](Program.cs); read them before changing any of it.

This puts one requirement on the consumer: standard error has to be read separately from standard output. Give descriptor 2 the same pipe - `2>&1`, or any other arrangement that merges them - and that descriptor holds a write end open of its own, so closing stdout delivers no EOF and the consumer waits for the process to exit instead. Measured on Linux with `--linger 3000`: EOF at 164 ms with the streams apart, at 3093 ms with them merged. Nothing fails; `--keep-stdout-open` and `--linger` just silently become the same thing.

### Both standard streams on one file, on Linux

`OpenStandardStream` wraps descriptors 1 and 2 in raw `FileStream`s, and on Unix a `FileStream` over a *seekable* handle keeps a position of its own starting at 0 and writes positionally, ignoring the kernel's shared file offset. Point both standard streams at the same regular file on Linux and the stderr stream writes over the payload from offset 0: measured with a 100-byte source, `> out.bin 2> err.log` came out intact, while both `> f.log 2>&1` and `> f.log 2> f.log` came out 100 bytes long beginning with the success message. The trigger is the two streams resolving to the same regular file, not the shell merging them - two independent opens of one file corrupt it just the same.

Pipes are not seekable, so writes to them stay sequential and are safe, and Windows writes through the handle's own file pointer, so neither is affected. On Linux, send the two streams to different files, or read stdout from a pipe.

### What it answers, and what it lets crash

A condition this program was written to meet gets an answer. A flag it doesn't know, a value out of range, a `--file` it can't read: the flag list on stderr, an empty stdout, exit code 1. Anything else is left to crash.

That is the deliberate half. If the program runs into something it wasn't written for - something whose mechanics we got wrong, or that behaves differently from how we understood it - an unhandled exception naming the cause is worth far more than a tidy exit code that buries it. A stub whose whole job is to be predictable must never let a run that went wrong pass for one that went right.

Three consequences are worth knowing, all of them measured rather than assumed.

**An invalid standard output or error descriptor aborts the run.** Hand it a closed or null stdout - `>&-`, or a parent with no console that doesn't redirect - and it dies instead of returning `--exit-code`. On Windows the failure lands at the open, as `ArgumentException: Invalid handle`; on Linux the stream opens quite happily and the first flush fails with `UnauthorizedAccessException` wrapping `IOException: Bad file descriptor`. When it's stderr that's invalid the failure comes even earlier, out of a static field initialiser, before `Main` has parsed anything. None of that is caught, and shouldn't be: a run that swallowed it would exit with the code it was asked for and no output, which is indistinguishable from a legitimate empty run. The crash says what actually happened.

**A consumer letting go of the pipe is the one write failure that is anticipated**, and it is caught - it raises `IOException` on both platforms, and the run carries on to its exit routine with the exit code it was given. The catch is wider than that single condition: a disk filling up under a redirected stdout would land in it too and be swallowed. Telling the two apart means matching on HResult or errno, which is fragile in a way of its own, and stdout is a pipe in every use this stub was built for. Knowingly left as it is.

**`--file` is checked by opening it**, not by asking whether it exists - existence says nothing about permissions or about someone else holding the file, and a check that answers the wrong question is worse than none. The file is opened once to check and again to read, so one that turns unreadable in between will still bring the run down. That race is left open on purpose: closing it means holding the handle across the parse, and an environment that shifts under a run mid-flight is exactly the sort of surprise that ought to be loud.
