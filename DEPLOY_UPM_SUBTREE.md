# UPM Subtree Deployment

This repo is a full Unity authoring project, but consumers only need the contents of `Packages/com.dream-tech-ex.enhanced-ui-framework/`. We publish releases as **git-subtree tags** so the install download stays small (~KBs instead of MBs).

| Method | Install Size | Speed | Use Case |
|---|---|---|---|
| Plain git URL (no tag) | Whole Unity project | Slow | Local hacking |
| **Git subtree tag (this method)** | Package only | Fast | Production |

## Pre-deploy checklist

- [ ] All changes committed to `main`
- [ ] `Packages/com.dream-tech-ex.enhanced-ui-framework/package.json` version bumped
- [ ] `Packages/com.dream-tech-ex.enhanced-ui-framework/CHANGELOG.md` updated
- [ ] No compile errors (open Unity, let it import, check the Console)
- [ ] `main` pushed to GitHub

## Release steps

### 1. Bump version

`Packages/com.dream-tech-ex.enhanced-ui-framework/package.json`:

```json
{ "version": "1.2.0" }
```

### 2. Add a CHANGELOG entry

```markdown
## [1.2.0] - YYYY-MM-DD

### Added
- …

### Fixed
- …
```

### 3. Commit & push

```bash
git add .
git commit -m "Release v1.2.0"
git push origin main
```

### 4. Run the deploy script

```bash
./deploy.sh --semver "1.2.0"
```

What it does (see [`deploy.sh`](deploy.sh)):

1. `git subtree split --prefix="Packages/com.dream-tech-ex.enhanced-ui-framework" --branch upm` — extract the package folder as a flat history on a temporary `upm` branch.
2. `git tag 1.2.0 upm` — tag that flat history.
3. `git push origin upm --tags` — publish the tag (tag captures the package-only tree).
4. `git push origin --delete upm` — drop the temp branch from the remote; the tag stays.
5. `git branch -D upm` — drop the local branch.

### 5. Verify

Open <https://github.com/TeamAcMong/Enhanced-UI-Framework/tags>, click the new tag — you should see only the package contents at the root (no `Assets/`, no `ProjectSettings/`).

## Install URL for consumers

```
https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.2.0
```

## Troubleshooting

### `tag already exists`

```bash
git tag -d 1.2.0                          # delete local
git push origin :refs/tags/1.2.0          # delete remote
./deploy.sh --semver "1.2.0"              # redeploy
```

### `refusing to update checked out branch`

You're sitting on `upm`. Switch back: `git checkout main`, then retry.

### Unity can't find the package

- Confirm the tag exists: `git ls-remote --tags origin`
- URL format must include the `#tag` suffix: `...Enhanced-UI-Framework.git#1.2.0`

## Versioning

Follow [SemVer](https://semver.org/):

- **MAJOR** (`2.0.0`) — breaking API change
- **MINOR** (`1.2.0`) — backwards-compatible feature
- **PATCH** (`1.1.1`) — backwards-compatible bug fix

## Don'ts

- ❌ Don't reuse a tag — treat published versions as immutable
- ❌ Don't rename the package (`name` field in `package.json`) after a public release — breaks all existing manifests
- ❌ Don't manually commit on the `upm` branch — it's overwritten on every deploy
- ❌ Don't delete published tags
