using System.Globalization;
using TMPro;
using Unity.MLAgents;
using UnityEngine;

public class StepCounterUI : MonoBehaviour
{
    private TextMeshProUGUI stepText;

    [SerializeField] private int numberOfEnvironments = 16;
    [SerializeField] private int decisionPeriod = 5;
    [SerializeField] private bool enforceStepLimit = true;
    [SerializeField] private long stepLimit = 100000;

    private NumberFormatInfo _spaceFormat;

    private void Start()
    {
        stepText = GetComponent<TextMeshProUGUI>();

        _spaceFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        _spaceFormat.NumberGroupSeparator = " ";
    }

    void Update()
    {
        long globalPythonSteps = ((long)Academy.Instance.StepCount / decisionPeriod) * numberOfEnvironments;

        stepText.text = $"Progress {globalPythonSteps.ToString("N0", _spaceFormat)} steps";

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