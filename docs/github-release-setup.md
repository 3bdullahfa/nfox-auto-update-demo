# GitHub Release Setup

This proof of concept uses GitHub Releases in a public releases-only update channel. The update channel is separate from the source repository.

```text
Source repository: 3bdullahfa/nfox-auto-update-demo
Update channel:    3bdullahfa/nfox-auto-update-channel
```

Verify GitHub CLI authentication:

```powershell
gh auth status
gh api user --jq .login
```

Create or verify the update channel repository:

```powershell
gh repo view 3bdullahfa/nfox-auto-update-channel
```

If the repository is missing, `tools/publish-update-channel-release.ps1` creates it with:

```powershell
gh repo create 3bdullahfa/nfox-auto-update-channel --public
```

The publishing script does not use `--source .` and does not push application source code to the update channel.

Build release artifacts:

```powershell
.\tools\build-release.ps1 -Version "1.0.2" -Owner "3bdullahfa" -Repo "nfox-auto-update-channel"
```

Publish:

```powershell
.\tools\publish-update-channel-release.ps1 -Owner "3bdullahfa" -Repo "nfox-auto-update-channel" -Version "1.0.2" -ReleaseTitle "NFOX Demo v1.0.2"
```

Expected release assets:

- `manifest.json`
- `NFOX.UpdatePackage-1.0.2.zip`
- `checksums.txt`

Use this stable URL in client/updater config:

```text
https://github.com/3bdullahfa/nfox-auto-update-channel/releases/latest/download/manifest.json
```

Private repositories require authenticated downloads, so do not use private GitHub Releases as a production update source for desktop clients unless a secure download strategy is added. Production systems should use a private update server, protected API, signed URLs, and package signing.
