# Architecture

The project follows the clean architecture approach. The following layers exist:

- Domain
- Service
- Infrastructure

More layers might be created once it's decided what they need to be.

# Use of tools

Use the built-in tools — Read, Write, Edit, Grep, Glob — for anything that touches files.
Never create or modify a file with a shell command: no `cat > file`, no heredocs, no `sed -i`,
no `python`/`perl`/`awk` used as an editor.

Use the shell for running the toolchain (`dotnet`, `git`) and for inspection the built-in
tools cannot do.

When a shell command names a path, write it out literally and keep it inside the working
directories. A path the shell computes cannot be checked against the read block and prompts
every time:

- no variables — `grep x "$f"`
- no command substitution — `grep x $(find . -name '*.cs')`
- no `~` — write the path in full
- no `find -exec` or `xargs` feeding another command
- no `cat "$f" | grep x` — that moves the read to `cat`

Prefer one literal root and let the tool recurse: `grep -rn --include="*.cs" "x" /abs/path`.

If something outside the working directories is genuinely needed, ask instead of retrying.