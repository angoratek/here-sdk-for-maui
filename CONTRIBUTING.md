# Contributing to HERE SDK for .NET MAUI

Thanks for your interest in contributing! This document covers the workflow,
conventions, and review process. For deeper project conventions (iOS binding
gotchas, Android binding patterns, architecture rationale), see
[`CLAUDE.md`](CLAUDE.md).

## Code of Conduct

Be respectful. Disagreements happen; ad-hominem doesn't. Assume good faith.

## Setting Up

Prerequisites:

- .NET 10 SDK (`10.0.201` or newer — see `global.json`)
- MAUI workloads: `dotnet workload install maui-android maui-ios`
- Android SDK (API 34+ recommended) — for RefApp + DeviceTests
- Xcode 15+ on macOS — for iOS NativeBridge + RefApp
- Node.js 20+ — for the Appium Android UI tests
- HERE SDK credentials (Access Key ID + Secret from developer.here.com)

Build everything:

```bash
./scripts/download-sdk.sh   # one-time: download HERE SDK archives to ~/.cache/heredl
./scripts/build.sh          # builds all bindings + MAUI library + RefApp, runs unit tests
./scripts/test.sh           # runs all 4 test suites
```

> If you already have the HERE SDK archives in `tmp/`, the build scripts will
> fall back to that path. `download-sdk.sh` is the preferred setup because it
> keeps the ~420 MB of SDK archives out of the repo working tree. Override the
> cache location with `export HERE_SDK_CACHE=/path/to/cache`.

## Repository Layout

See [`README.md`](README.md) for a high-level overview and
[`CLAUDE.md`](CLAUDE.md) for binding-specific guidance.

## Commit Style

- **One-liner only.** No multi-paragraph bodies.
- Format: `<type>: <imperative description>`.
- Types: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `ci:`, `chore:`,
  optionally with a scope: `fix(security): ...`.
- **Never** add `Co-Authored-By`, `Signed-off-by`, or other trailers.

Examples:

```
feat: add isoline renderer for routing
fix: pin docfx to v2.78 in publish.yml
test: add parity tests for GeoCoordinates equality
```

## Pull Request Workflow

1. Branch off `main`. Use a short, descriptive name (`fix-iOS-Mono-AOT`,
   `add-routing-isoline-tests`).
2. Make focused commits — one logical change per commit.
3. Run the local CI gate before pushing:
   ```bash
   dotnet test tests/HereSdk.Explore.Maui.Tests -c Release
   dotnet test tests/HereSdk.Explore.Maui.RefApp.UITests -c Release
   ./scripts/build.sh       # validates the full build matrix
   ```
4. Open a PR against `main`. Fill in the PR template:
   - What changed and why
   - Which test suites were run locally and their results
   - Any new public API surface (XML docs required)
5. CI runs `build.yml` (build + unit tests) and `ui-tests-android.yml`
   (Appium smoke). A maintainer will review and merge.

## Testing

This repo has 4 test suites, each with a distinct role:

| Project | Role | Runs in CI? |
|---|---|---|
| `tests/HereSdk.Explore.Maui.Tests` | Models, converters, service logic (no device) | ✅ |
| `tests/HereSdk.Explore.Maui.RefApp.UITests` | ViewModel commands + state transitions, in-process | ✅ (in `build.yml` via `scripts/test.sh`) |
| `tests/HereSdk.Explore.Maui.UITests` | Appium Android smoke on emulator | ✅ (`ui-tests-android.yml`) |
| `tests/HereSdk.Explore.Maui.DeviceTests` | Real SDK calls (Android builds; iOS blocked upstream) | Build-only in CI; runs locally |

For new tests:

- **Unit test** for any model/converters/service logic.
- **UI test** for any ViewModel command or page-level state change.
- **Device test** if you touch native bindings or platform handlers.

Naming: `{MethodName}_{Scenario}_{Expected}`.

## Public API Surface

- Every public type, method, property, and event must have an XML doc comment
  (`<summary>`, plus `<param>` / `<returns>` / `<exception>` where applicable).
- `TreatWarningsAsErrors=true` is enforced — missing docs will fail the build.
- Platform-specific types use `#if ANDROID` / `#if IOS` and must be marked
  `[SupportedOSPlatform("android")]` / `[SupportedOSPlatform("ios")]`.

## Release Process

Tagged releases (`v*`) trigger `publish.yml`, which:

1. Builds iOS NativeBridge + bindings + MAUI library
2. Packs all 3 NuGet packages
3. Publishes to NuGet.org using the `NUGET_API_KEY` repo secret
4. Generates DocFX and deploys to GitHub Pages

Maintainers handle the tag push; contributors don't need to bump versions.

## Where to Ask Questions

- **Bug reports / feature requests:** [GitHub Issues](../../issues)
- **Security issues:** see [`SECURITY.md`](SECURITY.md)
- **HERE SDK API questions:** the underlying SDK is documented at
  [developer.here.com](https://developer.here.com/) — this repo only wraps it.
