using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [SerializeField] private bool useTimeLimit = false;
    [SerializeField] private int limitHours = 0;
    [SerializeField] private int limitMinutes = 0;
    [SerializeField] private int limitSeconds = 0;

    private TextMeshProUGUI timerText;
    private float elapsedTime = 0f;

    void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        elapsedTime += Time.unscaledDeltaTime;
        UpdateTimerDisplay();
        CheckTimeLimit();
    }

    private void UpdateTimerDisplay()
    {
        int hours = Mathf.FloorToInt(elapsedTime / 3600);
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);

        if (timerText != null)
        {
            timerText.text = string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }
    }

    private void CheckTimeLimit()
    {
        if (!useTimeLimit) return;

        float limitInSeconds = (limitHours * 3600f) + (limitMinutes * 60f) + limitSeconds;

        if (elapsedTime >= limitInSeconds)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
