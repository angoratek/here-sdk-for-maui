# Support

## Where to ask

| Topic | Where to go |
|-------|-------------|
| Bug reports, feature requests, usage questions | [GitHub Issues](../../issues) |
| Security vulnerabilities | See [`SECURITY.md`](SECURITY.md) — **do not** file a public issue |
| HERE SDK API questions | [developer.here.com](https://developer.here.com/) — this repo only wraps the underlying SDK |
| Direct contact | info@angoratek.com |

## How to file a good issue

A well-formed issue gets answered faster. Include:

1. **Version** — output of `dotnet list package` showing `HereSdk.Explore.Maui` (and the platform binding if relevant).
2. **Platform** — Android (API level + device) or iOS (Xcode + device).
3. **HERE SDK version** — the underlying native SDK version this binding was built against (see `Version.props`).
4. **Repro** — minimal code sample or ref-app route to reproduce.
5. **Expected vs actual** — what you thought would happen, and what did.
6. **Logs** — `adb logcat` (Android) or Xcode console (iOS) snippets around the failure.

## What this repo does

This project provides .NET MAUI bindings for the HERE SDK Explore Edition. The
binding only re-exposes a subset of the underlying SDK; requests for new
wrappers should reference the corresponding native API by name. For SDK API
limitations or upstream behavior, contact HERE directly.

## Response time

This is a community-supported binding. There is no SLA on issue turnaround.
Maintainers triage weekly; PRs are reviewed on a best-effort basis.
