using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class TrainerMenu : EditorWindow
{
    private string runId = "my-test";
    private bool useForce = true;
    private bool forceCpu = true;

    [MenuItem("ML-Agents/Training Menu")]
    public static void ShowWindow()
    {
        GetWindow<TrainerMenu>("Training Menu");
    }

    private void OnEnable()
    {
        runId = PlayerPrefs.GetString("CurrentRunId", "my-test");
    }

    void OnGUI()
    {
        GUILayout.Label("Training Config", EditorStyles.boldLabel);

        string newRunId = EditorGUILayout.TextField("Run ID:", runId);

        if (newRunId != runId)
        {
            runId = newRunId;
            PlayerPrefs.SetString("CurrentRunId", runId);
            PlayerPrefs.Save();
        }

        useForce = EditorGUILayout.Toggle("Use --force:", useForce);
        forceCpu = EditorGUILayout.Toggle("Force CPU (--torch-device):", forceCpu);

        GUILayout.Space(10);

        if (GUILayout.Button("Enable Training"))
        {
            PlayerPrefs.SetString("CurrentRunId", runId);
            PlayerPrefs.Save();
            LaunchCmd(runId, useForce, forceCpu);
        }
    }

    private void LaunchCmd(string id, bool force, bool cpu)
    {
        string forceFlag = force ? " --force" : "";
        string cpuFlag = cpu ? " --torch-device cpu" : "";

        string cudaOverride = cpu ? "set CUDA_VISIBLE_DEVICES=-1 && " : "";

        string command = $"/k \"venv\\Scripts\\activate.bat &&{cudaOverride}mlagents-learn Assets\\Config\\Run.yaml --run-id={id} --results-dir=Assets\\Models{forceFlag}{cpuFlag}\"";

        ProcessStartInfo processInfo = new ProcessStartInfo();
        processInfo.FileName = "cmd.exe";
        processInfo.Arguments = command;
        processInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
        processInfo.CreateNoWindow = false;
        processInfo.UseShellExecute = true;

        Process.Start(processInfo);
    }
}