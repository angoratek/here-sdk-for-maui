## What

One-sentence description of the change.

## Why

What's the user-visible benefit, or which issue does it close?

## How

Concise description of the implementation. Link to design notes if applicable.

## Test plan

- [ ] `dotnet test tests/HereSdk.Explore.Maui.Tests -c Release` — passes
- [ ] `dotnet test tests/HereSdk.Explore.Maui.RefApp.UITests -c Release` — passes
- [ ] `dotnet build src/HereSdk.Explore.Maui -f net10.0-android -c Release` — passes
- [ ] `dotnet build src/HereSdk.Explore.Maui -f net10.0-ios -c Release` — passes
- [ ] Native binding touched? → `dotnet test tests/HereSdk.Explore.Maui.DeviceTests -f net10.0-android -c Release` passes
- [ ] UI test for any new ViewModel command or page-level state change
- [ ] Unit test for any model / converter / service logic change

## Public API

- [ ] All new public types / methods / properties / events have XML doc comments (`<summary>`, `<param>`, `<returns>`, `<exception>` as applicable)
- [ ] Platform-specific types are marked `[SupportedOSPlatform("android")]` / `[SupportedOSPlatform("ios")]` and use `#if ANDROID` / `#if IOS`

## Checklist

- [ ] Branch is off `main`
- [ ] Commits are one-liners in the format `<type>: <description>` (no body, no trailers)
- [ ] No `Co-Authored-By` / `Signed-off-by` trailers
- [ ] No edits to unrelated code
- [ ] No new warnings under `TreatWarningsAsErrors=true`
