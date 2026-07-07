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

    [SerializeField] private string csvFileName = "results.csv";
    private string _csvFilePath;

    [SerializeField] private int lastEpisodesCount = 100;
    private string _summaryFilePath;

    [SerializeField] private int aggregationInterval = 100;
    private string _aggregatedCsvFilePath;
    private int _chunkRunnerWins = 0;
    private int _chunkPursuerWins = 0;

    private int _totalEpisodes = 0;
    private int _runnerWins = 0;
    private int _pursuerWins = 0;

    private Queue<bool> _runnerWinHistory = new Queue<bool>();
    private Queue<bool> _pursuerWinHistory = new Queue<bool>();

    public void SaveEpisodeStats(string winner, float runnerJitter, float pursuerJitter, float nearMissTime)
    {
        _totalEpisodes++;

        bool runnerWon = (winner == "Runner");
        bool pursuerWon = (winner == "Pursuer");

        if (runnerWon) _runnerWins++;
        if (pursuerWon) _pursuerWins++;

        if (runnerWon) _chunkRunnerWins++;
        if (pursuerWon) _chunkPursuerWins++;

        _runnerWinHistory.Enqueue(runnerWon);
        if (_runnerWinHistory.Count > lastEpisodesCount)
        {
            _runnerWinHistory.Dequeue();
        }

        _pursuerWinHistory.Enqueue(pursuerWon);
        if (_pursuerWinHistory.Count > lastEpisodesCount)
        {
            _pursuerWinHistory.Dequeue();
        }

        Academy.Instance.StatsRecorder.Add("Movement/Runner_Jitter", runnerJitter);
        Academy.Instance.StatsRecorder.Add("Movement/Pursuer_Jitter", pursuerJitter);
        Academy.Instance.StatsRecorder.Add("Gameplay/Near_Miss_Time", nearMissTime);

        string dataLine = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0},{1},{2:F3},{3:F3},{4:F2}\n",
            _totalEpisodes, winner, runnerJitter, pursuerJitter, nearMissTime);

        File.AppendAllText(_csvFilePath, dataLine);

        SaveAccuracyInInterval();

        UpdateTextDisplay(winner, runnerJitter, pursuerJitter, nearMissTime);
    }

    private void SaveAccuracyInInterval()
    {
        if (_totalEpisodes % aggregationInterval == 0)
        {
            float chunkRunnerAcc = (_chunkRunnerWins / (float)aggregationInterval);
            float chunkPursuerAcc = (_chunkPursuerWins / (float)aggregationInterval);

            string aggLine = $"{_totalEpisodes}," +
                             $"{chunkPursuerAcc.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                             $"{chunkRunnerAcc.ToString(System.Globalization.CultureInfo.InvariantCulture)}\n";

            File.AppendAllText(_aggregatedCsvFilePath, aggLine);

            _chunkRunnerWins = 0;
            _chunkPursuerWins = 0;
        }
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            StartLoggingToCSV();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StartLoggingToCSV()
    {
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
        _aggregatedCsvFilePath = Path.Combine(directoryPath, $"Aggregated_{fileNameWithoutExt}_{timestamp}.csv");

        File.WriteAllText(_csvFilePath, "Episode,Winner,RunnerJitter,PursuerJitter,NearMissTime\n");
        File.WriteAllText(_aggregatedCsvFilePath, "Episode,Pursuer Accuracy,Runner Accuracy\n");
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
        if (_totalEpisodes > 0)
        {
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