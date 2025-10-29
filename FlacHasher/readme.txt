TABLE OF CONTENTS

Purpose of this program
1. Audio Decoder Set up
2. Hash Verification: Hashfile Structure
3. Command-line Interface
3.1. Commands and Syntax
3.2 Genetal parameter/configuration information
3.3. Configuration profiles
3.4. Command Referece
3.4.1. Hashing
3.4.2. Verification

===========================================================
PURPOSE OF THIS PROGRAM

This program hashes actual (uncompressed) audio content of a file without any of the metadata. It produces the same result for the same audio data regardless of metadata tags, compression level and even format/codec (provided it's lossless). This allows fingerprinting of actual audio data without having to "freeze" metadata tags after and comparison of two files on pure audio data level.
It also does verification: a comparison of hashes of specified files against what's recorded in a text file (here referred to as "hashfile").

IMPORTANT!
It uses external 3rd party audio decoders that have to be obtained and installed by the user. It can be configured to decode any type of file as long as the decoder has an executable command-line interface and can accept input either via std-input or command-line parameters.
By default, it tries to use FLAC decoder. To use any other decoder, it has to be configured manually -- see parameter/configuration reference below.

===========================================================
1. AUDIO DECODER SET UP

- The program needs to know the path to the Audio Decoder executable file. It can be a full path or just a file name, in which case it gets looked up in paths configured in PATH environment variable.
If not specified, the value is assumed to be "flac.exe", in which case it uses default decoder parameters (unless explicitly defined).

- Multiple Audio decoder configurations can be stored as decoder Profiles. For that, a decoder section must be named [Decoder.YourName] -- eg, [Decoder.FLAC]. A default decoder can be stored simply as [Decoder].
A go-to profile can then be specified in "Profile.Decoder".

Refer to the application's help page for info on required parameters.

===========================================================
2. HASH VERIFICATION: HASHFILE STRUCTURE

A "hashfile" stores previously calculated file hashes for future verification.
It can either contain just the hashes (with no file names) or filenames with hashes.
If it's just hash values, the order in which they are recorded matters.
If a hashfile contains file names, then HashfileEntrySeparator must be configured (tells where file name ends and hash value starts).

Any character sequence can be used as a separator between filename-hash, except for quote and new-line chars. However, if filename and/or hash value contains said separator char sequence, then these values must be presented in quotes. For example:
  "file 1.flac" 34579878657567543 -- separator is Space char (" ") and filename contains a space, therefore filename must be in quotes
  "file-1.flac"-34579878657567543 -- separator is a dash and filename contains a dash, therefore filename must be in quotes
There's no harm in always putting filenames in quotes.

A single space char " " for separator means any amount of space and tab chars are treated as separator. For example, both of these work when separator is a single space " ":
  file1.flac 34579878657567543
  "file1 1.flac"	34579878657567543

===========================================================
3. COMMAND-LINE INTERFACE

3.1. Commands and Syntax

The following commands are available:
  - hash (operation name is optional, it executes by default)
  - verify

Syntax:
  flachasher [command] [--param=value]

for example:
  flachasher hash --input=c:\muzak\slts.flac --algorithm=md5

Where value is "true"/"false", the value can be completely omitted to provide "true", eg:
  flachash --fail-fast

===========================================================
3.2. GENERAL PARAMETER/CONFIGURATION INFO

- Many aspects can also be configured via the settings file, which means they can be configured once. In such case, a value can be provided either way. A relevant settings file key will be listed right next to the command-line parameter, where available.

  Settings file: settings.ini

- Command-line parameter values with spaces: put the whole value in quotes, eg:
    flachasher --input="c:\flac files\01 track.flac"

- Parameters that take arrays of values (eg input or decoder parameters) have to be separated by a semi-color (;), eg:
    --input=track1.flac;track2.flac

  Alternatively, multi-value command-line parameters can be repeated to provide multiple values, eg:
    --input=track1.flac --input=track2.flac

  The latter doesn't apply to the settings file.

===========================================================
3.3. CONFIGURATION PROFILES

- Decoder profile can be chosen via --profile-decoder | Profile.Decoder
- Hashing profile can be chosen via --profile-hashing | Profile.Hashing

- To create general configuration profiles, place relevant configuration keys in [YourProfile] section.
- A profile can be chosen via --profile | Profile

===========================================================
3.4. COMMAND REFERENCE
===========================================================
3.4.1. HASHING

Resulting hashes get written to std-out stream (whereas the rest of the info - to std-error), which means that they can be written to a file using standard output redirection commands.
Multiple files can be hashed at a time.

The most important inputs here are:
  1) input audio files
  2) audio decoder -- configuration explained in a dedicated section
  3) hashing algorithm
  4) output format

===========================================================
3.4.2. VERIFICATION

The most important inputs here are:
  1) hashfile
  2) actual audio files
  3) audio decoder -- configuration explained in a dedicated section
  4) hashing algorithm
  5) hashfile format

- Hashfile has to be either/or:
a) a full path to the file - then target files get taken from the same directory as the hashfile, or from Input Directory specified separately;
b) relative path (just the file name) - then Input Directory has to be specified (otherwise, hashfile and target files get looked up in "current" directory).

If a Hashfile is not explicitly specified, it gets looked up in the Input Directory using HashfileExtensions.

- Target file names get taken from the hashfile and are looked up in:
a) Hashfile directory, if hashfile paramter is a full path
b) Input Directory, if specified -- based on TargetFileExtension
c) Input Files, if specified

- For auto-algorithm inferral from hashfile extension, hashing algorithm and hashfile extension maps can be stored as profiles, named [Hashing.Name], eg [Hashing.MD5].