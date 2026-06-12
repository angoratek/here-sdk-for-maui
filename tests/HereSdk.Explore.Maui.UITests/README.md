# HERE SDK MAUI — Appium UI Tests (Android)

End-to-end UI smoke tests for the HERE SDK MAUI reference app, driven by
[Appium 2.x](https://appium.io/) with the
[UIAutomator2](https://github.com/appium/appium-uiautomator2-driver) driver.
The pattern is adapted from the official
[dotnet/maui-samples `BasicAppiumNunitSample`](https://github.com/dotnet/maui-samples/tree/main/10.0/UITesting).

## What is covered

- The 4 ref-app tabs (Explore, Directions, Traffic, Tools) load and present
  their key controls (search entry, origin/destination entries, flow/incidents
  chips, drawing tools, dark-mode toggle).
- Search entries accept text input.

The suite **does not** assert on map rendering. The HERE SDK needs OpenGL ES
2.0 with EGL initialization, which can fail under the swiftshader software
GPU that CI emulators boot with. Map behaviour is covered by the in-process
`RefApp.UITests` and `DeviceTests` projects.

## Local execution

Prerequisites:
- .NET 10 SDK
- `maui-android` workload (`dotnet workload install maui-android`)
- Android SDK + an emulator (or a physical device). The Android SDK Manager
  is bundled with Android Studio or the standalone `commandlinetools`.
- Node.js 20+
- Appium 2.x and the UIAutomator2 driver:
  ```bash
  npm i -g appium@2.5.0
  appium driver install uiautomator2
  ```

Steps:
```bash
# 1. Build the ref app (Debug — uses the auto-generated debug keystore)
dotnet build src/HereSdk.Explore.Maui.RefApp -f net10.0-android -c Debug

# 2. Boot an emulator (or connect a device). Wait for sys.boot_completed.
emulator -avd <your_avd_name> -no-snapshot &     # or: adb devices
adb wait-for-device shell 'while [[ -z $(getprop sys.boot_completed) ]]; do sleep 1; done'

# 3. Install the APK
adb install -r src/HereSdk.Explore.Maui.RefApp/bin/Debug/net10.0-android/com.here.explore.maui.refapp-Signed.apk

# 4. (Optional) start Appium manually so you can watch its log:
appium --port 4723 &

# 5. Run the tests
dotnet test tests/HereSdk.Explore.Maui.UITests -c Release
```

`AppiumSetup.OneTimeSetUp` will start an Appium server on 127.0.0.1:4723 if
one isn't already running, and create a single `AndroidDriver` shared by all
tests in the assembly.

## CI execution

See `.github/workflows/ui-tests-android.yml`. The workflow:

1. Installs the Android SDK + the `system-images;android-34;google_apis;x86_64`
   system image.
2. Creates an AVD named `testavd` (Pixel 5 profile).
3. Boots the emulator headlessly with `swiftshader_indirect` for software
   rendering.
4. Builds the ref app in Debug (no signing config required).
5. Installs the APK on the emulator.
6. Installs Node 20 + Appium 2.5.0 + UIAutomator2 driver.
7. Runs the test suite.
8. Uploads the `screenshots/` directory as a CI artifact.

## Adding tests

Each tab has its own page-object test file in `PageObjects/`. To add a new
test:

1. Add an `AutomationId` to the XAML element you want to drive.
2. Add a new test method in the matching `PageObjects/*Tests.cs` file.
3. Use `FindUIElement("<id>")` to locate the element and `Screenshot("<name>")`
   to capture state for the CI artifact.

## Limitations

- The HERE SDK needs a valid access key/secret at app startup. CI builds use
  the values in `src/HereSdk.Explore.Maui.RefApp/appsettings.json`. If you
  fork this repository, replace those with a test-only key.
- The `dotnet test` invocation expects the Appium server to be reachable on
  127.0.0.1:4723. Override the host/port by editing `AppiumSetup.cs`.
- The tests do not currently cover physical iOS devices — that would require
  a separate Appium XCUITest driver and Apple signing infrastructure.
