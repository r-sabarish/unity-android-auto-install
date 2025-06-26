#if UNITY_EDITOR && UNITY_ANDROID
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class ADBPostBuildInstaller
{
    [PostProcessBuild]

    public static void OnPostBuild(BuildTarget target, string pathToBuiltProject)
    {
        // returns when not an android platform or .apk
        if (target != BuildTarget.Android || !pathToBuiltProject.EndsWith(".apk"))
        {
            return;
        }

        // getting apk path
        string apkPath = pathToBuiltProject;

        // getting selected devices
        string selected = EditorPrefs.GetString("ADB_SelectedDevices", "");
        if (string.IsNullOrEmpty(selected))
        {
            UnityEngine.Debug.LogWarning("No selected devices saved. Skipping Android Auto Build install.");
            return;
        }

        // getting sdkpath
        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        if (string.IsNullOrEmpty(sdkPath))
        {
            UnityEngine.Debug.LogError("Android SDK path not found.");
            return;
        }

        // getting adb path
        string adb = Path.Combine(sdkPath, "platform-tools",
            Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb");

        // seperate selected device id's
        string[] devices = selected.Split(',');

        // process install all selected devices
        foreach (var device in devices)
        {
            if (string.IsNullOrWhiteSpace(device)) continue;

            UnityEngine.Debug.Log($"Installing APK to {device}...");

            // install using adb install -r 
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = adb,
                Arguments = $"-s {device.Trim()} install -r \"{apkPath}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process proc = Process.Start(psi))

            // getting adb output
            using (StreamReader reader = proc.StandardOutput)
            {
                string result = reader.ReadToEnd();
                UnityEngine.Debug.Log($"ADB install result for {device}:\n{result}");
            }
        }
    }
}
#endif
