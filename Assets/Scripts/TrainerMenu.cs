using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class TrainerMenu : EditorWindow
{
    private string runId = "my-test";
    private bool useForce = true;

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

        GUILayout.Space(10);

        if (GUILayout.Button("Enable Training"))
        {
            PlayerPrefs.SetString("CurrentRunId", runId);
            PlayerPrefs.Save();
            LaunchCmd(runId, useForce);
        }
    }

    private void LaunchCmd(string id, bool force)
    {
        string forceFlag = force ? " --force" : "";
        string command = $"/k \"venv\\Scripts\\activate.bat && mlagents-learn Assets\\Config\\Run.yaml --run-id={id}{forceFlag}\"";

        ProcessStartInfo processInfo = new ProcessStartInfo();
        processInfo.FileName = "cmd.exe";
        processInfo.Arguments = command;
        processInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
        processInfo.CreateNoWindow = false;
        processInfo.UseShellExecute = true;

        Process.Start(processInfo);
    }
}