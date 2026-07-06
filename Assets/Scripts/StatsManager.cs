using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using Unity.MLAgents;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager Instance { get; private set; }

    public TextMeshProUGUI statsText;

    public string csvFileName = "results.csv";
    private string _csvFilePath;

    public int lastEpisodesCount = 100;
    private string _summaryFilePath;

    private int _totalEpisodes = 0;
    private int _runnerWins = 0;
    private int _pursuerWins = 0;

    private Queue<bool> _runnerWinHistory = new Queue<bool>();
    private Queue<bool> _pursuerWinHistory = new Queue<bool>();


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

            _summaryFilePath = Path.Combine(directoryPath, $"Summary_{fileNameWithoutExt}_{timestamp}.txt");
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

        bool runnerWon = (winner == "Runner");
        bool pursuerWon = (winner == "Pursuer");

        if (runnerWon) _runnerWins++;
        if (pursuerWon) _pursuerWins++;

        // --- Logika Kolejki (Historia ostatnich epok) ---

        // Zapisujemy wynik uciekiniera i usuwamy najstarszy, jeœli przekroczyliœmy limit
        _runnerWinHistory.Enqueue(runnerWon);
        if (_runnerWinHistory.Count > lastEpisodesCount)
        {
            _runnerWinHistory.Dequeue();
        }

        // To samo dla ³owcy
        _pursuerWinHistory.Enqueue(pursuerWon);
        if (_pursuerWinHistory.Count > lastEpisodesCount)
        {
            _pursuerWinHistory.Dequeue();
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

    private void OnApplicationQuit()
    {
        // Sprawdzamy, czy w ogóle rozegrano jak¹kolwiek epokê
        if (_totalEpisodes > 0)
        {
            // Liczymy œredni¹ tylko z zawartoœci kolejki (np. ze 100 ostatnich rund)
            float runnerAvg = _runnerWinHistory.Count > 0 ? (float)_runnerWinHistory.Count(w => w) / _runnerWinHistory.Count * 100f : 0f;
            float pursuerAvg = _pursuerWinHistory.Count > 0 ? (float)_pursuerWinHistory.Count(w => w) / _pursuerWinHistory.Count * 100f : 0f;

            string summary = $"--- FINAL SUMMARY ---\n" +
                             $"Total Episodes Played: {_totalEpisodes}\n" +
                             $"Overall Runner Wins: {_runnerWins}\n" +
                             $"Overall Pursuer Wins: {_pursuerWins}\n\n" +
                             $"--- RECENT PERFORMANCE (Last {lastEpisodesCount} episodes) ---\n" +
                             $"Runner Recent Accuracy: {runnerAvg:F2}%\n" +
                             $"Pursuer Recent Accuracy: {pursuerAvg:F2}%\n";

            File.WriteAllText(_summaryFilePath, summary);
        }
    }
}