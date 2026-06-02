# NutriVision

NutriVision is a .NET MAUI mobile nutrition tracking application built for the Food and Drink coursework. The project focuses on a complete user workflow and visible use of mobile hardware features on Android and Windows.

## Overview

The main idea of NutriVision is to connect food recognition, nutrition analysis, local history, statistics, accessibility, and hardware interaction into one complete workflow:

`Scan -> Analyze -> Save -> Review -> Speak -> Locate`

The app is designed for coursework demonstration on:

- Android phone
- Android tablet
- Windows

## Features

- Capture food images inside the app
- Control flash inside the app before capture
- Use microphone input for food names
- Retrieve and display user location
- Read summaries and results aloud with text-to-speech
- Save records locally with SQLite
- Query history and review previous records
- Show nutrition statistics and summaries
- Display compass heading in real time
- Display accelerometer X, Y, and Z values
- Trigger rescan logic with shake detection
- Provide vibration or haptic feedback on successful actions
- Support cloud-first recognition with local fallback

## Hardware Features

The project implementation includes these mobile hardware related features:

1. Camera
2. Flash control
3. Microphone
4. Geolocation / Geocoding
5. Text-to-Speech
6. Vibration / Haptic feedback
7. Shake detection
8. Compass
9. Accelerometer

## Stack

- .NET MAUI
- XAML UI
- C#
- CommunityToolkit.Mvvm
- CommunityToolkit.Maui.Camera
- SQLite (`sqlite-net-pcl`)
- Dependency Injection via `MauiProgram`

## Architecture

The project follows `MVVM + Services + DI + SQLite`.

Code is organized into:

- `Models`
- `ViewModels`
- `Views`
- `Services`
- `Helpers`
- `Resources`

### Responsibilities

- `Views`: UI layout and interaction surface
- `ViewModels`: state, commands, presentation logic
- `Services`: hardware access, recognition, storage, speech, sensors, and integration logic
- `Models`: app data structures and domain types
- `Resources`: styles, colors, assets

### Design Intention

The architecture separates UI logic from device and service logic so the app is easier to maintain, extend, debug, and demonstrate. Hardware access is wrapped behind service interfaces instead of being called directly from pages.

## Core Workflow

The main end-to-end workflow is:

1. User opens the scan flow
2. User captures a food image or provides voice input
3. Recognition runs with cloud-first strategy
4. If cloud recognition fails, local fallback is used
5. Nutrition data is generated
6. Result can be saved to SQLite history
7. History and statistics update from saved data
8. Speech and location feedback can be triggered from the app

## Accessibility

The project includes accessibility-oriented settings and feedback mechanisms:

- Text-to-speech output
- Dynamic font sizing
- Theme switching
- High contrast mode
- User-friendly error messages instead of technical exceptions

## Reliability Strategy

To improve demonstration stability, the project uses the following approach:

- Cloud-first recognition with local fallback
- Permission checks before hardware access
- `try/catch` around hardware and external service calls
- Friendly feedback for unavailable device capabilities
- Release APK validation for real Android device testing

## Challenges And Solutions

This section records important problems encountered during implementation and how they were solved.

### 1. Android emulator and device differences

Problem:
Some hardware features behaved differently between Windows, emulator, and real Android devices, especially speech, sensors, and camera-related behavior.

Solution:
The app was tested on both emulator and real Android hardware. Features that depend on real sensors or device services were separated into dedicated services so behavior could be validated and adjusted more easily.

### 2. Debug deployment instability on real device

Problem:
Manual installation of the Debug package caused runtime loading issues on Android.

Solution:
Real-device validation was switched to a Release publish workflow. A signed Release APK was generated and used for installation testing instead of relying only on debug deployment.

### 3. XAML startup crash from missing resources

Problem:
The app crashed on startup because shared XAML resources such as `AppCardShadow` and `AppAccentColor` were referenced but not defined globally.

Solution:
Missing global resources were added into `App.xaml`, and shared UI resources were checked again to avoid repeated startup-time crashes.

### 4. Flash requirement from coursework marking

Problem:
Using only the built-in system camera was not enough to count as flash implementation according to the assessment explanation.

Solution:
The camera workflow was moved to an embedded in-app preview using `CommunityToolkit.Maui.Camera`, and flash control was implemented directly in the app UI.

### 5. Voice input environment dependency

Problem:
Microphone and speech recognition support depended on the Android image and installed device services.

Solution:
The app keeps user-friendly failure handling and was validated with a real-device deployment path instead of relying only on emulator support.

## Run

### Windows

Open the solution in Visual Studio and run the Windows target.

### Android

Use Visual Studio for emulator deployment or generate a Release APK for real-device testing.

Example publish command:

```powershell
dotnet publish "D:\dev\fooddrink-lllllzzzz224\NutriVision\NutriVision.csproj" -f net9.0-android -c Release
```

Example install command:

```powershell
& "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" -s <device-id> install -r "D:\dev\fooddrink-lllllzzzz224\NutriVision\bin\Release\net9.0-android\publish\com.companyname.nutrivision-Signed.apk"
```

## Project Thinking

This project is not only about implementing features. The design goal is to demonstrate:

- a complete mobile workflow
- visible use of hardware features
- structured architecture
- graceful failure handling
- stable real-device execution

In other words, the project was developed with both coursework marking criteria and practical maintainability in mind.

## Repository Strategy

- Main working branch: `lz-21906394`
- Small-scope changes grouped into readable PRs
- Implementation was refined iteratively through architecture, hardware integration, device testing, and UI fixes
