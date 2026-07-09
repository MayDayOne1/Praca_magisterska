using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutoTesterMenu : EditorWindow
{
    private int totalRuns = 5;

    private const string PrefIsRunning = "AutoTest_IsRunning";
    private const string PrefTotalRuns = "AutoTest_TotalRuns";
    private const string PrefCurrentRun = "AutoTest_CurrentRun";

    static AutoTesterMenu()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("ML-Agents/Auto Tester")]
    public static void ShowWindow()
    {
        GetWindow<AutoTesterMenu>("Auto Tester");
    }

    void OnGUI()
    {
        GUILayout.Label("Inference Tests Automatization", EditorStyles.boldLabel);

        totalRuns = EditorGUILayout.IntSlider("Runs Count:", totalRuns, 1, 20);

        GUILayout.Space(10);

        bool isRunning = EditorPrefs.GetBool(PrefIsRunning, false);

        if (isRunning)
        {
            int current = EditorPrefs.GetInt(PrefCurrentRun, 0);
            int total = EditorPrefs.GetInt(PrefTotalRuns, 0);
            EditorGUILayout.HelpBox($"Test in progress...\nRun {current}/{total}.", MessageType.Info);

            if (GUILayout.Button("Abort tests", GUILayout.Height(30)))
            {
                StopAutoTests();
            }
        }
        else
        {
            if (GUILayout.Button("Begin tests", GUILayout.Height(30)))
            {
                StartAutoTests();
            }
        }
    }

    private void StartAutoTests()
    {
        EditorPrefs.SetBool(PrefIsRunning, true);
        EditorPrefs.SetInt(PrefTotalRuns, totalRuns);
        EditorPrefs.SetInt(PrefCurrentRun, 1);

        Debug.Log($"<color=cyan>[AutoTester]</color> Begin run 1/{totalRuns}.");

        EditorApplication.isPlaying = true;
    }

    private static void StopAutoTests()
    {
        EditorPrefs.SetBool(PrefIsRunning, false);
        EditorApplication.isPlaying = false;
        Debug.Log("<color=red>[AutoTester]</color> Tests abort by user.");
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            bool isRunning = EditorPrefs.GetBool(PrefIsRunning, false);
            if (isRunning)
            {
                int current = EditorPrefs.GetInt(PrefCurrentRun, 0);
                int total = EditorPrefs.GetInt(PrefTotalRuns, 0);

                if (current < total)
                {
                    current++;
                    EditorPrefs.SetInt(PrefCurrentRun, current);
                    Debug.Log($"<color=cyan>[AutoTester]</color> Begin run {current}/{total}...");

                    EditorApplication.delayCall += () =>
                    {
                        EditorApplication.isPlaying = true;
                    };
                }
                else
                {
                    Debug.Log("<color=green>[AutoTester] Tests successful.</color>");
                    EditorPrefs.SetBool(PrefIsRunning, false);
                }
            }
        }
    }
}