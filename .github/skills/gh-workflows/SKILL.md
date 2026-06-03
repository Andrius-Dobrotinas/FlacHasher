---
name: gh-workflows
description: 'Drive GitHub Actions workflows on remote repos via the gh CLI from the terminal. USE FOR: list workflows, view workflow YAML, trigger / dispatch / run workflow_dispatch workflows, find the just-triggered run, watch run progress, fetch run logs, debug failed jobs, rerun failed jobs, cancel run, download artifacts, enable/disable a workflow, check gh auth status and scopes. Works without MCP. DO NOT USE FOR: authoring or editing workflow YAML files (regular code editing); local git operations (use git directly); GitHub PR/issue management (use the GitHub PR extension tools or gh pr/gh issue).'
---

# GitHub Workflows via `gh` CLI

Use the `gh` CLI from a terminal to drive GitHub Actions workflows on remote repos. No MCP is required.

## Prerequisites

- `gh --version` succeeds.
- `gh auth status` shows an active account with scopes `repo` + `workflow`.
  - If `workflow` is missing: `gh auth refresh -s workflow,repo`.
  - Multi-account: `gh auth switch` to change the active one.

## Conventions

- **Shell**: all snippets are PowerShell (Windows). The `gh` invocations themselves are shell-agnostic, but watch for: `Start-Sleep -Seconds N` (use `sleep N` in bash), backtick `` ` `` line continuation (use `\` in bash), and `$env:VAR` (use `$VAR` in bash).
- Always pass `--repo <owner>/<name>` so commands work outside the repo's local clone.
- Identify a workflow by its **filename** (eg `build-publish.yml`) or **numeric ID**. Display names with spaces are awkward to quote — prefer ID or filename.
- `--ref <branch|tag|sha>` controls which ref the workflow runs against (only meaningful for `workflow_dispatch`).

## Standard Playbook

1. **List** — `gh workflow list --repo R --all` → confirm it exists.
2. **Inspect** — `gh workflow view ID --repo R --ref BRANCH --yaml` → check `workflow_dispatch` is present and what `inputs` are required.
3. **Trigger** — `gh workflow run ID --repo R --ref BRANCH [-f k=v ...]`.
4. **Locate run** — `Start-Sleep 3; gh run list --repo R --workflow=ID --branch BRANCH --limit 3` → grab the new run ID (newest, smallest `AGE`).
5. **Observe / debug** — `gh run watch RUN_ID --repo R --exit-status` *or* `gh run view RUN_ID --repo R --log-failed`.

## Testing a workflow file change

GitHub runs the YAML from the remote ref — local edits are invisible until pushed. When a workflow file was modified locally, wrap the Standard Playbook with these steps:

1. **Commit on a temp branch** so the remote sees the change without touching the original branch:
   ```powershell
   git checkout -b temp/workflow-test
   git add <workflow-file>
   git commit -m "temp: test workflow change"
   git push origin temp/workflow-test
   ```
2. **Trigger and observe** using Standard Playbook steps 3–5, with `--ref temp/workflow-test`.
3. **Clean up** once testing is done (pass or fail):
   ```powershell
   git reset HEAD~1                         # uncommit — changes return to working tree
   git checkout <original-branch>           # carries uncommitted changes back
   git branch -D temp/workflow-test
   git push origin --delete temp/workflow-test
   ```
   The workflow file changes are now back on the original branch as uncommitted.

## Discover

```powershell
# Workflows registered on the repo (independent of branch)
gh workflow list --repo <owner>/<repo> --all

# Inspect YAML on a specific ref — required to know if inputs are needed
gh workflow view <id|file.yml> --repo <owner>/<repo> --ref <branch> --yaml
```

Check the YAML for:
- `on: workflow_dispatch:` — required to trigger manually. If absent, you cannot run it remotely; only push/PR/schedule will.
- `inputs:` block under `workflow_dispatch` — every required input must be supplied via `-f key=value`.

## Trigger

```powershell
# No inputs
gh workflow run <id|file.yml> --repo <owner>/<repo> --ref <branch>

# With inputs
gh workflow run <id|file.yml> --repo <owner>/<repo> --ref <branch> `
  -f environment=staging -f version=1.2.3
```

`gh workflow run` returns immediately after creating the dispatch event; it does **not** print the run ID.

## Find the run you just triggered

The dispatch is async — there's a brief delay before the run appears.

```powershell
Start-Sleep -Seconds 3
gh run list --repo <owner>/<repo> --workflow=<id|file.yml> --branch <branch> --limit 5
```

The newest row (smallest `AGE`) with `EVENT=workflow_dispatch` is yours. Capture its ID.

## Inspect outcomes / errors

```powershell
# Snapshot status
gh run view <run-id> --repo <owner>/<repo>

# Stream until completion (blocks); non-zero exit on failure
gh run watch <run-id> --repo <owner>/<repo> --exit-status

# Logs of failed steps only — best for debugging
gh run view <run-id> --repo <owner>/<repo> --log-failed

# Full logs
gh run view <run-id> --repo <owner>/<repo> --log

# Per-job drilldown
gh run view --job=<job-id> --repo <owner>/<repo> --log
```

## Manage runs

```powershell
gh run cancel <run-id> --repo <owner>/<repo>
gh run rerun  <run-id> --repo <owner>/<repo>            # rerun all
gh run rerun  <run-id> --repo <owner>/<repo> --failed   # rerun only failed jobs
gh run download <run-id> --repo <owner>/<repo> -n <artifact-name> -D ./out
```

## Enable / disable a workflow

```powershell
gh workflow enable  <id|file.yml> --repo <owner>/<repo>
gh workflow disable <id|file.yml> --repo <owner>/<repo>
```

## Gotchas

- **`workflow_dispatch` must exist on the target ref.** If `master` doesn't have it in the YAML at that ref, the trigger 422s — even if `main` has it.
- **`gh workflow list` shows registrations, not files on a branch.** A workflow can appear there but be absent from the ref you're targeting. Use `gh workflow view --ref <branch>` to confirm.
- **Required inputs without `-f` cause a client-side rejection** before dispatch. Read the YAML first.
- **Run IDs are not returned by `gh workflow run`.** Always follow up with `gh run list` filtered by workflow + branch.
- **Names with spaces** ("Build & Publish Pipeline"): quote them, or just use the ID/filename.
- **Scopes**: triggering needs `workflow`; reading private repo runs needs `repo`. Re-auth with `gh auth refresh -s workflow,repo` if missing.
