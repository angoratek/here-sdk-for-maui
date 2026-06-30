---
name: Bug report
about: Report incorrect behavior or a failure in the HERE SDK MAUI binding
title: "[bug] "
labels: ["bug", "triage"]
assignees: []
---

## Summary

One-sentence description of the bug.

## Environment

- **Binding version** (from `dotnet list package`): `HereSdk.Explore.Maui` `x.y.z`
- **Platform binding version**: `HereSdk.Explore.Android.Binding` / `HereSdk.Explore.iOS.Binding` `x.y.z`
- **Platform**: Android / iOS
- **Device**: emulator or physical device; model
- **OS version**: e.g. Android 14, iOS 17.5
- **MAUI workload**: `dotnet workload list` (paste output)
- **Xcode version** (iOS only): `xcodebuild -version`
- **.NET SDK**: `dotnet --version`

## Repro steps

1. ...
2. ...
3. ...

## Expected behavior

What you thought would happen.

## Actual behavior

What actually happened, with the relevant logs / exception / screenshot.

## Logs

Paste `adb logcat` (Android) or Xcode console (iOS) output around the failure,
plus the exception message and stack trace if any.

## Workaround

If you found a workaround, describe it.
