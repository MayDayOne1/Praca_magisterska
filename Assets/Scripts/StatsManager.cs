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

    [Range(1f, 100f)]
    [SerializeField] private float timeScaleMultiplier = 1f;

    [SerializeField] private int lastEpisodesCount = 100;
    private string _summaryFilePath;

    [SerializeField] private int aggregationInterval = 100;
    private string _aggregatedCsvFilePath;
    private int _chunkRunnerWins = 0;
    private int _chunkPursuerWins = 0;
    private int _chunkDraws = 0;

    private float _chunkRunnerJitter = 0f;
    private float _chunkPursuerJitter = 0f;
    private float _chunkNearMissTime = 0f;

    [SerializeField] private int testEpisodesLimit = 100;
    private bool _isInferenceMode = false;

    private int _totalEpisodes = 0;
    private int _runnerWins = 0;
    private int _pursuerWins = 0;
    private int _drawCount = 0;

    private float _totalRunnerJitter = 0f;
    private float _totalPursuerJitter = 0f;
    private float _totalNearMissTime = 0f;

    private int _chunkEpisodeSteps = 0;
    private long _totalEpisodeSteps = 0;

    private Queue<bool> _runnerWinHistory = new Queue<bool>();
    private Queue<bool> _pursuerWinHistory = new Queue<bool>();
    private Queue<bool> _drawsHistory = new Queue<bool>();
    private Queue<float> _runnerJitterHistory = new Queue<float>();
    private Queue<float> _pursuerJitterHistory = new Queue<float>();
    private Queue<float> _nearMissHistory = new Queue<float>();
    private Queue<int> _episodeStepsHistory = new Queue<int>();

    private float _realElapsedTime = 0f;

    public void SaveEpisodeStats(string winner, float runnerJitter, float pursuerJitter, float nearMissTime, int episodeSteps)
    {
        _totalEpisodes++;

        bool runnerWon = (winner == "Runner");
        bool pursuerWon = (winner == "Pursuer");
        bool isDraw = (winner == "Draw");

        if (runnerWon) _runnerWins++;
        if (pursuerWon) _pursuerWins++;
        if (isDraw) _drawCount++;

        if (runnerWon) _chunkRunnerWins++;
        if (pursuerWon) _chunkPursuerWins++;
        if (isDraw) _chunkDraws++;

        _totalRunnerJitter += runnerJitter;
        _totalPursuerJitter += pursuerJitter;
        _totalNearMissTime += nearMissTime;
        _totalEpisodeSteps += episodeSteps;

        _chunkRunnerJitter += runnerJitter;
        _chunkPursuerJitter += pursuerJitter;
        _chunkNearMissTime += nearMissTime;
        _chunkEpisodeSteps += episodeSteps;


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

        _drawsHistory.Enqueue(isDraw);
        if (_drawsHistory.Count > lastEpisodesCount)
        {
            _drawsHistory.Dequeue();
        }

        _runnerJitterHistory.Enqueue(runnerJitter);
        _pursuerJitterHistory.Enqueue(pursuerJitter);
        _nearMissHistory.Enqueue(nearMissTime);
        _episodeStepsHistory.Enqueue(episodeSteps);

        if (_runnerJitterHistory.Count > lastEpisodesCount)
        {
            _runnerJitterHistory.Dequeue();
            _pursuerJitterHistory.Dequeue();
            _nearMissHistory.Dequeue();
            _episodeStepsHistory.Dequeue();
        }

        float runnerAcc = (_runnerWins / (float)_totalEpisodes);
        float pursuerAcc = (_pursuerWins / (float)_totalEpisodes);
        float drawsPercentage = (_drawCount / (float)_totalEpisodes);

        //Academy.Instance.StatsRecorder.Add("Movement/Runner_Jitter", runnerJitter);
        //Academy.Instance.StatsRecorder.Add("Movement/Pursuer_Jitter", pursuerJitter);
        //Academy.Instance.StatsRecorder.Add("Gameplay/Near_Miss_Time", nearMissTime);

        string dataLine = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "{0},{1},{2:F3},{3:F3},{4:F3},{5:F3},{6:F3},{7:F2},{8:F2}\n",
            _totalEpisodes, winner, runnerAcc, pursuerAcc, drawsPercentage, runnerJitter, pursuerJitter, nearMissTime, episodeSteps);

        File.AppendAllText(_csvFilePath, dataLine);

        SaveAccuracyInInterval();

        UpdateTextDisplay(winner, runnerJitter, pursuerJitter, nearMissTime);

        // if(_isInferenceMode && _totalEpisodes >= testEpisodesLimit) StopTest();
    }

    private void SaveAccuracyInInterval()
    {
        if (_totalEpisodes % aggregationInterval == 0)
        {
            float chunkRunnerAcc = (_chunkRunnerWins / (float)aggregationInterval);
            float chunkPursuerAcc = (_chunkPursuerWins / (float)aggregationInterval);
            float chunkDrawsPercentage = (_chunkDraws / (float)aggregationInterval);

            float chunkRunnerJitterAvg = _chunkRunnerJitter / aggregationInterval;
            float chunkPursuerJitterAvg = _chunkPursuerJitter / aggregationInterval;
            float chunkNearMissAvg = _chunkNearMissTime / aggregationInterval;
            float chunkEpisodeStepsAvg = (float)_chunkEpisodeSteps / aggregationInterval;

            string aggLine = $"{_totalEpisodes}," +
                             $"{chunkRunnerAcc.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                             $"{chunkPursuerAcc.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                             $"{chunkDrawsPercentage.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                             $"{chunkRunnerJitterAvg.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                             $"{chunkPursuerJitterAvg.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                             $"{chunkNearMissAvg.ToString(System.Globalization.CultureInfo.InvariantCulture)}," +
                             $"{chunkEpisodeStepsAvg.ToString(System.Globalization.CultureInfo.InvariantCulture)}\n";

            File.AppendAllText(_aggregatedCsvFilePath, aggLine);

            _chunkRunnerWins = 0;
            _chunkPursuerWins = 0;
            _chunkDraws = 0;

            _chunkRunnerJitter = 0f;
            _chunkPursuerJitter = 0f;
            _chunkNearMissTime = 0f;
            _chunkEpisodeSteps = 0;
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

    private void Start()
    {
        _isInferenceMode = !Academy.Instance.IsCommunicatorOn;
    }

    private void Update()
    {
        _realElapsedTime += Time.unscaledDeltaTime;

        if (!_isInferenceMode) return;

        if (Mathf.Abs(Time.timeScale - timeScaleMultiplier) > 0.01f)
        {
            Time.timeScale = timeScaleMultiplier;
        }
    }

    private void StopTest()
    {
        Debug.Log($"Reached limit of {testEpisodesLimit} epochs. Stopping...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        
    }

    private void StartLoggingToCSV()
    {
        string currentRunId = PlayerPrefs.GetString("CurrentRunId", "default_run");

        string baseDirectoryPath = Path.Combine(Application.dataPath, "..", "csv");
        string runDirectoryPath = Path.Combine(baseDirectoryPath, currentRunId);

        if (!System.IO.Directory.Exists(baseDirectoryPath))
        {
            System.IO.Directory.CreateDirectory(baseDirectoryPath);
        }
        if (!System.IO.Directory.Exists(runDirectoryPath))
        {
            System.IO.Directory.CreateDirectory(runDirectoryPath);
        }

        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileNameWithoutExt = Path.GetFileNameWithoutExtension(csvFileName);
        string finalFileName = $"{fileNameWithoutExt}_{timestamp}.csv";

        _summaryFilePath = Path.Combine(runDirectoryPath, $"Summary_{fileNameWithoutExt}_{timestamp}.txt");
        _csvFilePath = Path.Combine(runDirectoryPath, finalFileName);
        _aggregatedCsvFilePath = Path.Combine(runDirectoryPath, $"Aggregated_{fileNameWithoutExt}_{timestamp}.csv");

        File.WriteAllText(_csvFilePath, "Episode,Winner,RunnerAcc,PursuerAcc,DrawsPercentage,RunnerJitter,PursuerJitter,NearMissTime,EpisodeSteps\n");
        File.WriteAllText(_aggregatedCsvFilePath, "Episode,RunnerAcc,PursuerAcc,DrawsPercentage,RunnerJitter,PursuerJitter,NearMissTime,EpisodeStepsAvg\n");
    }

    private void UpdateTextDisplay(string winnerName, float runnerJitter, float pursuerJitter, float nearMissTime)
    {
        if (statsText == null) return;

        float runnerAcc = _totalEpisodes > 0 ? ((float)_runnerWins / _totalEpisodes) * 100f : 0f;
        float pursuerAcc = _totalEpisodes > 0 ? ((float)_pursuerWins / _totalEpisodes) * 100f : 0f;
        float drawsPerc = _totalEpisodes > 0 ? ((float)_drawCount / _totalEpisodes) * 100f : 0f;

        statsText.text = $"LAST EPISODE STATS\n" +
                         $"Winner: <b>{winnerName}</b>\n" +
                         $"Runner Jitter: {runnerJitter:F2}\n" +
                         $"Pursuer Jitter: {pursuerJitter:F2}\n" +
                         $"Near Miss Time: {nearMissTime:F1}s" +
                         "\n" +
                         $"Runner Acc: {runnerAcc:F1}% ({ _runnerWins}/{ _totalEpisodes})\n" +
                         $"Pursuer Acc: {pursuerAcc:F1}% ({ _pursuerWins}/{ _totalEpisodes})\n" +
                         $"Draw %: {drawsPerc:F1}% ({_drawCount}/{_totalEpisodes})";


    }

    private void OnApplicationQuit()
    {
        if (_totalEpisodes > 0)
        {
            float runnerAvg = _runnerWinHistory.Count > 0 ? (float)_runnerWinHistory.Count(w => w) / _runnerWinHistory.Count * 100f : 0f;
            float pursuerAvg = _pursuerWinHistory.Count > 0 ? (float)_pursuerWinHistory.Count(w => w) / _pursuerWinHistory.Count * 100f : 0f;
            float drawAvg = _drawsHistory.Count > 0 ? (float)_drawsHistory.Count(w => w) / _drawsHistory.Count * 100f : 0f;

            float overallRunnerJitterAvg = _totalRunnerJitter / _totalEpisodes;
            float overallPursuerJitterAvg = _totalPursuerJitter / _totalEpisodes;
            float overallNearMissAvg = _totalNearMissTime / _totalEpisodes;
            float overallEpisodeStepsAvg = (float)_totalEpisodeSteps / _totalEpisodes;

            float recentRunnerJitterAvg = _runnerJitterHistory.Count > 0 ? _runnerJitterHistory.Average() : 0f;
            float recentPursuerJitterAvg = _pursuerJitterHistory.Count > 0 ? _pursuerJitterHistory.Average() : 0f;
            float recentNearMissAvg = _nearMissHistory.Count > 0 ? _nearMissHistory.Average() : 0f;
            float recentEpisodeStepsAvg = _episodeStepsHistory.Count > 0 ? (float)_episodeStepsHistory.Average() : 0f;

            int hours = Mathf.FloorToInt(_realElapsedTime / 3600);
            int minutes = Mathf.FloorToInt((_realElapsedTime % 3600) / 60);
            int seconds = Mathf.FloorToInt(_realElapsedTime % 60);
            string formattedTime = $"{hours:00}:{minutes:00}:{seconds:00}";

            string summary = $"--- FINAL SUMMARY ---\n" +
                             $"Total Episodes Played: {_totalEpisodes}\n" +
                             $"Total Elapsed Time: {formattedTime}\n" +
                             $"Overall Runner Wins: {_runnerWins}\n" +
                             $"Overall Pursuer Wins: {_pursuerWins}\n" +
                             $"Overall Draws: {_drawCount}\n" +
                             $"Overall Runner Jitter Avg: {overallRunnerJitterAvg:F2}\n" +
                             $"Overall Pursuer Jitter Avg: {overallPursuerJitterAvg:F2}\n" +
                             $"Overall Near Miss Time Avg: {overallNearMissAvg:F2}s\n" +
                             $"Overall Episode Length Avg: {overallEpisodeStepsAvg:F0} steps\n\n" +
                             $"--- RECENT PERFORMANCE (Last {lastEpisodesCount} episodes) ---\n" +
                             $"Runner Recent Accuracy: {runnerAvg:F2}%\n" +
                             $"Pursuer Recent Accuracy: {pursuerAvg:F2}%\n" +
                             $"Draws Percentage: {drawAvg:F2}%\n" +
                             $"Recent Runner Jitter Avg: {recentRunnerJitterAvg:F2}\n" +
                             $"Recent Pursuer Jitter Avg: {recentPursuerJitterAvg:F2}\n" +
                             $"Recent Near Miss Time Avg: {recentNearMissAvg:F2}s\n" +
                             $"Recent Episode Length Avg: {recentEpisodeStepsAvg:F0} steps\n";

            File.WriteAllText(_summaryFilePath, summary);
        }
    }
}