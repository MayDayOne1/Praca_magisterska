using UnityEngine;
using TMPro;
using System.IO;
using Unity.MLAgents;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    [Header("UI References")]
    public TextMeshProUGUI statsText;

    [Header("Export Settings")]
    public string csvFileName = "ExperimentStats.csv";
    private string _csvFilePath;

    private int _episodeCounter = 0;

    private void Awake()
    {
        // Konfiguracja Singletona - zapewnia, ¿e istnieje tylko jeden StatsManager
        if (Instance == null)
        {
            Instance = this;
            _csvFilePath = Path.Combine(Application.dataPath, "..", csvFileName);

            // Jeœli plik nie istnieje, stwórz go i dodaj nag³ówki kolumn
            if (!File.Exists(_csvFilePath))
            {
                File.WriteAllText(_csvFilePath, "Episode,Winner,RunnerJitter,PursuerJitter,NearMissTime\n");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Jedyna metoda zapisu - przyjmuje wszystkie dane w argumentach (jest w pe³ni bezstanowa)
    public void SaveEpisodeStats(string winnerName, float runnerJitter, float pursuerJitter, float nearMissTime)
    {
        _episodeCounter++;

        // 1. EXPORT TO TENSORBOARD (U¿ywamy argumentów z metody, a nie zmiennych klasy!)
        Academy.Instance.StatsRecorder.Add("Movement/Runner_Jitter", runnerJitter);
        Academy.Instance.StatsRecorder.Add("Movement/Pursuer_Jitter", pursuerJitter);
        Academy.Instance.StatsRecorder.Add("Gameplay/Near_Miss_Time", nearMissTime);

        // 2. EXPORT TO CSV
        // Zapis z u¿yciem InvariantCulture, by zawsze u¿ywaæ kropki dla u³amków
        string dataLine = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0},{1},{2:F3},{3:F3},{4:F2}\n",
            _episodeCounter, winnerName, runnerJitter, pursuerJitter, nearMissTime);

        File.AppendAllText(_csvFilePath, dataLine);

        // 3. UPDATE ON-SCREEN UI
        UpdateTextDisplay(winnerName, runnerJitter, pursuerJitter, nearMissTime);
    }

    private void UpdateTextDisplay(string winnerName, float runnerJitter, float pursuerJitter, float nearMissTime)
    {
        if (statsText == null) return;

        statsText.text = $"<color=#FFFF00>LAST EPISODE STATS</color>\n" +
                         $"Winner: <b>{winnerName}</b>\n" +
                         $"Runner Jitter: {runnerJitter:F2}\n" +
                         $"Pursuer Jitter: {pursuerJitter:F2}\n" +
                         $"Near Miss Time: {nearMissTime:F1}s";
    }
}