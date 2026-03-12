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

    void OnGUI()
    {
        GUILayout.Label("Training Config", EditorStyles.boldLabel);

        runId = EditorGUILayout.TextField("Run ID:", runId);
        useForce = EditorGUILayout.Toggle("Use --force:", useForce);

        GUILayout.Space(10);

        if (GUILayout.Button("Enable Training"))
        {
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