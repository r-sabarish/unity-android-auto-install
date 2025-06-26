# Unity-Android-Auto-Install

**Unity-Android-Auto-Install** is a Unity Editor tool built to improve Android development workflows by **automating APK installation** to connected devices.

> ⚠️ Works only with **Android platform** selected in Unity.

---

## Purpose

To improve the development workflow by **automatically installing the built APK** to selected Android devices **immediately after a successful build**.

---

## Features

- 📱 Detect and list connected Android devices
- ✅ Select/deselect devices for APK installation
- 🌐 Connect devices over IP
- 📦 Select and manually install APKs via the editor
- ⚙️ Auto-install APK to selected devices post-build

---

## How to Use

1. Copy this Git repository URL (with release) : https://github.com/r-sabarish/unity-apk-installer.git
2. Open your Unity project.
3. Go to **Window** → **Package Manager**.
4. Click the **+** button (top left corner) → **Add package from Git URL...**
5. Paste the Git URL and click **Add**.

---

## Requirements

### Unity Editor  
Ensure Unity is installed **with Android Build Support**, which can be selected during installation via Unity Hub.

---

### ADB (Android Debug Bridge)  
ADB is a command-line tool that enables communication with Android devices. It's used to:

- Install APKs  
- Run shell commands  
- Debug apps on connected devices  

**Included with the Android SDK**, typically located in:  
`<Unity Install Folder>/Editor/Data/PlaybackEngines/AndroidPlayer/SDK/platform-tools`

> 🔧 Make sure the SDK path is set in Unity via:  
**Unity > Preferences > External Tools > Android SDK Path**  
(Key used internally: `EditorPrefs: AndroidSdkRoot`)

**ADB Documentation:**  
[https://developer.android.com/studio/command-line/adb](https://developer.android.com/studio/command-line/adb)

---

### Android Device(s)  
Devices must be connected via **USB** or over **Wi-Fi (IP connection)**.

When connecting for the first time:

- Enable **Developer Options** on your Android device  
- Turn on **USB debugging**  
- **Authorize the device** when prompted with the *"Allow USB debugging?"* dialog on the device

---


## License

This project is available under [MIT](LICENSE) .

---

## Contribution

Pull requests and contributions are welcome!

