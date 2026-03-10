using UnityEngine;
using UnityEditor;
using System.Diagnostics;

public class TensorBoardMenu
{
    [MenuItem("ML-Agents/Launch TensorBoard")]
    public static void Launch()
    {
        ProcessStartInfo processInfo = new ProcessStartInfo("cmd.exe", "/c \"venv\\Scripts\\activate.bat && tensorboard --logdir results\"");
        processInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
        Process.Start(processInfo);

        Application.OpenURL("http://localhost:6006");
    }
}