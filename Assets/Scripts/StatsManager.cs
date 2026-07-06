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
    public string csvFileName = "results.csv";
    private string _csvFilePath;

    private int _totalEpisodes = 0;
    private int _runnerWins = 0;
    private int _pursuerWins = 0;


    private void Awake()
    {
        // Konfiguracja Singletona - zapewnia, ¿e istnieje tylko jeden StatsManager
        if (Instance == null)
        {
            Instance = this;

            string directoryPath = Path.Combine(Application.dataPath, "..", "csv");
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(csvFileName);
            string finalFileName = $"{fileNameWithoutExt}_{timestamp}.csv";

            _csvFilePath = Path.Combine(directoryPath, finalFileName);

            // Zapisujemy nag³ówki - zgodnie z proœb¹, BEZ kolumn Accuracy
            File.WriteAllText(_csvFilePath, "Episode,Winner,RunnerJitter,PursuerJitter,NearMissTime\n");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Jedyna metoda zapisu - przyjmuje wszystkie dane w argumentach (jest w pe³ni bezstanowa)
    public void SaveEpisodeStats(string winner, float runnerJitter, float pursuerJitter, float nearMissTime)
    {
        _totalEpisodes++;

        if (winner.Contains("Runner"))
        {
            _runnerWins++;
        }
        else if (winner.Contains("Pursuer"))
        {
            _pursuerWins++;
        }

        // 1. EXPORT TO TENSORBOARD (U¿ywamy argumentów z metody, a nie zmiennych klasy!)
        Academy.Instance.StatsRecorder.Add("Movement/Runner_Jitter", runnerJitter);
        Academy.Instance.StatsRecorder.Add("Movement/Pursuer_Jitter", pursuerJitter);
        Academy.Instance.StatsRecorder.Add("Gameplay/Near_Miss_Time", nearMissTime);

        // 2. EXPORT TO CSV
        // Zapis z u¿yciem InvariantCulture, by zawsze u¿ywaæ kropki dla u³amków
        string dataLine = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0},{1},{2:F3},{3:F3},{4:F2}\n",
            _totalEpisodes, winner, runnerJitter, pursuerJitter, nearMissTime);

        File.AppendAllText(_csvFilePath, dataLine);

        // 3. UPDATE ON-SCREEN UI
        UpdateTextDisplay(winner, runnerJitter, pursuerJitter, nearMissTime);
    }

    private void UpdateTextDisplay(string winnerName, float runnerJitter, float pursuerJitter, float nearMissTime)
    {
        if (statsText == null) return;

        float runnerAcc = _totalEpisodes > 0 ? ((float)_runnerWins / _totalEpisodes) * 100f : 0f;
        float pursuerAcc = _totalEpisodes > 0 ? ((float)_pursuerWins / _totalEpisodes) * 100f : 0f;

        statsText.text = $"LAST EPISODE STATS\n" +
                         $"Winner: <b>{winnerName}</b>\n" +
                         $"Runner Jitter: {runnerJitter:F2}\n" +
                         $"Pursuer Jitter: {pursuerJitter:F2}\n" +
                         $"Near Miss Time: {nearMissTime:F1}s" +
                         "\n" +
                         $"Runner Acc: {runnerAcc:F1}% ({ _runnerWins}/{ _totalEpisodes})\n" +
                         $"Pursuer Acc: {pursuerAcc:F1}% ({ _pursuerWins}/{ _totalEpisodes})";


    }
}