# GitHub Release Setup

This proof of concept uses GitHub Releases as a public test update server.

Verify GitHub CLI authentication:

```powershell
gh auth status
gh api user --jq .login
```

Create or verify the repository:

```powershell
gh repo view <OWNER>/nfox-auto-update-demo
gh repo create nfox-auto-update-demo --public --source . --remote origin --push
```

Build release artifacts:

```powershell
.\tools\build-release.ps1 -Version "1.0.1" -Owner "<OWNER>" -Repo "nfox-auto-update-demo"
```

Publish:

```powershell
.\tools\publish-github-release.ps1 -Owner "<OWNER>" -Repo "nfox-auto-update-demo" -Version "1.0.1" -ReleaseTitle "NFOX Demo v1.0.1"
```

Expected release assets:

- `manifest.json`
- `NFOX.DemoApp-1.0.1.zip`
- `NFOX.Migrations-1.0.1.zip`
- `checksums.txt`

Use this URL in updater config:

```text
https://github.com/<OWNER>/nfox-auto-update-demo/releases/download/v1.0.1/manifest.json
```

Private repositories require authenticated downloads, so do not use private GitHub Releases as a production update source for desktop clients unless a secure download strategy is added.
