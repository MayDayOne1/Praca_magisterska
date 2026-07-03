using UnityEngine;
using TMPro;
using System.Collections;

public class AccuracyManager : MonoBehaviour
{
    public static AccuracyManager Instance { get; private set; }

    private TextMeshProUGUI accText;

    private int runnerSuccesses = 0;
    private int runnerTotalAttempts = 0;

    private int pursuerSuccesses = 0;
    private int pursuerTotalAttempts = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        accText = GetComponent<TextMeshProUGUI>();
        StartCoroutine(UpdateUIRoutine());
    }


    public void RegisterRunnerAttempt(bool isSuccess)
    {
        runnerTotalAttempts++;
        if (isSuccess)
        {
            runnerSuccesses++;
        }
    }

    public void RegisterPursuerAttempt(bool isSuccess)
    {
        pursuerTotalAttempts++;
        if (isSuccess)
        {
            pursuerSuccesses++;
        }
    }

    private IEnumerator UpdateUIRoutine()
    {
        while (true)
        {
            UpdateTextDisplay();
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    private void UpdateTextDisplay()
    {
        if (accText == null) return;

        float runnerAcc = runnerTotalAttempts > 0 ? ((float)runnerSuccesses / runnerTotalAttempts) * 100f : 0f;
        float pursuerAcc = pursuerTotalAttempts > 0 ? ((float)pursuerSuccesses / pursuerTotalAttempts) * 100f : 0f;

        string displayText = $"Runner Acc {runnerAcc:F1}% ({runnerSuccesses}/{runnerTotalAttempts})\n" +
                             $"Pursuer Acc {pursuerAcc:F1}% ({pursuerSuccesses}/{pursuerTotalAttempts})";

        accText.text = displayText;
    }
}
