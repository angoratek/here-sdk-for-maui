# Security Policy

## Supported Versions

| Version | Supported          |
|---------|--------------------|
| 4.25.5.x | ✅ Active          |
| < 4.25  | ❌ End of life     |

## Reporting a Vulnerability

**Please do not open a public GitHub issue for security vulnerabilities.**

Report privately by emailing **security@angoratek.com**. Include:

- A clear description of the vulnerability and its impact
- Steps to reproduce, or a minimal proof-of-concept
- Affected version(s) and commit SHA(s) if known

We aim to:

- Acknowledge your report within **5 business days**
- Provide an initial assessment within **15 business days**
- Coordinate disclosure and release a fix within **90 days** of acknowledgement
  (faster for actively-exploited issues)

We follow responsible disclosure. We will credit reporters in the CHANGELOG
entry for the fix unless you ask to remain anonymous.

## Scope

This project wraps the HERE SDK Explore Edition (v4.25.5.0). Vulnerabilities
specific to the underlying HERE SDK should be reported to HERE directly via
[developer.here.com](https://developer.here.com/). This security policy covers
only the binding/wrapper code in this repository.

## Known Operational Concerns

- **Credentials handling.** The reference app (`HereSdk.Explore.Maui.RefApp`)
  reads HERE SDK credentials from `appsettings.json`. The committed file
  contains placeholders (`YOUR_ACCESS_KEY_ID`); for local development, copy
  it to `appsettings.Local.json` (gitignored) and put real credentials there.
  Do not commit real credentials.
- **iOS device tests** are not run in CI as of this writing. The CI matrix is
  build-only on `macos-latest` for the iOS binding.
