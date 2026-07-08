using UnityEngine;
using TMPro;
using Unity.MLAgents;

public class StepCounterUI : MonoBehaviour
{
    private TextMeshProUGUI stepText;

    [SerializeField] private int numberOfEnvironments = 16;
    [SerializeField] private int decisionPeriod = 5;
    [SerializeField] private bool enforceStepLimit = true;
    [SerializeField] private long stepLimit = 100000;

    private void Start()
    {
        stepText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        long globalPythonSteps = ((long)Academy.Instance.StepCount / decisionPeriod) * numberOfEnvironments;

        stepText.text = $"Progress {globalPythonSteps:N0} steps";

        if (enforceStepLimit && globalPythonSteps >= stepLimit)
        {
            enforceStepLimit = false;

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
        }
    }
}