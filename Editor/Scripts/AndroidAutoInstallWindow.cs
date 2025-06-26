#if UNITY_EDITOR && UNITY_ANDROID
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class AndroidAutoInstallWindow : EditorWindow
{
    private List<string> connectedDevices = new List<string>();
    private List<bool> selectedDevices = new List<bool>();
    private string ipToConnect = "192.168.";
    private string apkFilePath = "";

    [MenuItem("Tools/Android Auto Install")]
    public static void ShowWindow()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            GetWindow<AndroidAutoInstallWindow>("Android Auto Install");
        }
        else
        {
            EditorUtility.DisplayDialog("Platform Mismatch", "This tool only works when Android is the active build target !", "OK");
        }
    }

    void OnFocus()
    {
        FetchConnectedDevices();
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("Connect using IP", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        ipToConnect = EditorGUILayout.TextField("IP Address", ipToConnect);
        if (GUILayout.Button("Connect", GUILayout.Width(100)))
        {
            ConnectToDevice(ipToConnect);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("Refresh Devices", GUILayout.Height(25)))
        {
            FetchConnectedDevices();
        }

        GUILayout.Space(15);
        GUILayout.Label("Connected Devices", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < connectedDevices.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            selectedDevices[i] = EditorGUILayout.Toggle(selectedDevices[i], GUILayout.Width(20));
            GUILayout.Label(connectedDevices[i]);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(10);
        GUILayout.Label("Selected Devices", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");
        for (int i = 0; i < connectedDevices.Count; i++)
        {
            if (selectedDevices[i])
            {
                GUILayout.Label("+ " + connectedDevices[i]);
            }
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(15);
        GUILayout.Label("APK Installer", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical("box");

        if (GUILayout.Button("Select APK File", GUILayout.Height(25)))
        {
            string selected = EditorUtility.OpenFilePanel("Select .apk", "", "apk");
            if (!string.IsNullOrEmpty(selected) && selected.EndsWith(".apk"))
            {
                apkFilePath = selected;
            }
        }

        if (!string.IsNullOrEmpty(apkFilePath))
        {
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("Selected APK:\n" + Path.GetFileName(apkFilePath), MessageType.Info);

            GUILayout.Space(5);
            if (GUILayout.Button("Install APK", GUILayout.Height(30)))
            {
                InstallApkToSelectedDevices(apkFilePath);
            }

            if (GUILayout.Button("Remove Selected APK", GUILayout.Height(22)))
            {
                apkFilePath = "";
            }
        }

        EditorGUILayout.EndVertical();
    }
    // saving selected device
    private void SaveSelectedDevices()
    {
        List<string> selected = new List<string>();
        for (int i = 0; i < connectedDevices.Count; i++)
        {
            if (selectedDevices[i])
                selected.Add(connectedDevices[i]);
        }

        string joined = string.Join(",", selected);
        EditorPrefs.SetString("ADB_SelectedDevices", joined);
    }


    // handle attched devices using adb
    private void FetchConnectedDevices()
    {
        connectedDevices.Clear();
        selectedDevices.Clear();

        try
        {
            // getting sdk
            string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            if (string.IsNullOrEmpty(sdkPath))
            {
                UnityEngine.Debug.LogError("Android SDK path not found in EditorPrefs.");
                return;
            }

            // getting adb
            string adbExecutable = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";
            string adbPath = Path.Combine(sdkPath, "platform-tools", adbExecutable);
            if (!File.Exists(adbPath))
            {
                UnityEngine.Debug.LogError("ADB not found at: " + adbPath);
                return;
            }

            // adb devices to list attached devices
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = "devices",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // formatting output
            using (Process process = Process.Start(psi))
            using (StreamReader reader = process.StandardOutput)
            {
                string output = reader.ReadToEnd();
                string[] lines = output.Split('\n');

                string saved = EditorPrefs.GetString("ADB_SelectedDevices", "");
                string[] savedDevices = saved.Split(',');

                foreach (string line in lines)
                {
                    string trimmedLine = line.Trim();
                    if (trimmedLine.EndsWith("device") && trimmedLine.Contains("\t"))
                    {
                        string deviceId = trimmedLine.Split('\t')[0].Trim();
                        connectedDevices.Add(deviceId);
                        selectedDevices.Add(savedDevices.Contains(deviceId));
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to fetch ADB devices: " + ex.Message);
        }
    }


    // handle for connect using IP
    private void ConnectToDevice(string ip)
    {
        if (string.IsNullOrEmpty(ip)) return;

        try
        {
            string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            string adbExecutable = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";
            string adbPath = Path.Combine(sdkPath, "platform-tools", adbExecutable);

            var psi = new ProcessStartInfo
            {
                FileName = adbPath,
                Arguments = $"connect {ip}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(psi))
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                if (result.Contains("connected to"))
                {
                    EditorUtility.DisplayDialog("ADB Connect", $"Successfully connected to {ip}", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("ADB Connect Failed", result, "OK");
                }
            }

            FetchConnectedDevices();
        }
        catch (System.Exception ex)
        {
            UnityEngine.Debug.LogError("Failed to connect via ADB: " + ex.Message);
        }
    }

    // handle for install selected app
    private void InstallApkToSelectedDevices(string apkPath)
    {
        if (!File.Exists(apkPath))
        {
            EditorUtility.DisplayDialog("APK Error", "Selected APK file does not exists !", "OK");
            return;
        }

        // Check at least one device is device
        if (!selectedDevices.Any(sel => sel))
        {
            EditorUtility.DisplayDialog("APK Installation", "No devices selected!", "OK");
            return;
        }

        string sdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        string adbExecutable = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";
        string adbPath = Path.Combine(sdkPath, "platform-tools", adbExecutable);

        foreach (var (deviceId, selected) in connectedDevices.Zip(selectedDevices, (id, sel) => (id, sel)))
        {
            if (!selected) continue;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = $"-s {deviceId} install -r \"{apkPath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(psi))
                using (StreamReader reader = process.StandardOutput)
                using (StreamReader errorReader = process.StandardError)
                {
                    string output = reader.ReadToEnd();
                    string error = errorReader.ReadToEnd();
                    if (output.Contains("Success"))
                    {
                        UnityEngine.Debug.Log($"Installed on {deviceId}: Success");
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"Failed on {deviceId}: {output}\n{error}");
                    }
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to install APK on {deviceId}: {ex.Message}");
            }
        }

        // EditorUtility.DisplayDialog("APK Installation", "APK installation finished. Check Console for details.", "OK");
    }
}
#endif
