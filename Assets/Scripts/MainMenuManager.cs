using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.MLAgents.Policies;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button runnerButton;
    [SerializeField] private Button pursuerButton;
    [SerializeField] private Image runnerBorder;
    [SerializeField] private Image pursuerBorder;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI playButtonText;
    [SerializeField] private TextMeshProUGUI specificObjective;

    public enum AvailablePlayers
    {
        None,
        Runner,
        Pursuer
    }

    public AvailablePlayers playerChosen = AvailablePlayers.None;

    public void SetHumanPlayer(bool isRunnerHuman, bool isPursuerHuman)
    {
        if (isRunnerHuman)
        {
            GameConfig.RunnerMode = GameConfig.ControlMode.Human;
            GameConfig.PursuerMode = GameConfig.ControlMode.AI;
        }

        if (isPursuerHuman)
        {
            GameConfig.RunnerMode = GameConfig.ControlMode.AI;
            GameConfig.PursuerMode = GameConfig.ControlMode.Human;
        }

        if (!isRunnerHuman && !isPursuerHuman)
        {
            GameConfig.RunnerMode = GameConfig.ControlMode.AI;
            GameConfig.PursuerMode = GameConfig.ControlMode.AI;
        }
    }

    public void ToggleRunner()
    {
        runnerBorder.enabled = !runnerBorder.enabled;
        pursuerBorder.enabled = false;

        if (runnerBorder.enabled)
        {
            playerChosen = AvailablePlayers.Runner;
            specificObjective.text = "Get to the goal, avoid the Pursuer.";
            playButtonText.text = "Play as Runner";
            SetHumanPlayer(isRunnerHuman: true, isPursuerHuman: false);
        }
        else
        {
            playerChosen = AvailablePlayers.None;
            playButtonText.text = "Spectate";
            specificObjective.text = "";

            SetHumanPlayer(isRunnerHuman: false, isPursuerHuman: false);
        }

    }

    public void TogglePursuer()
    {
        pursuerBorder.enabled = !pursuerBorder.enabled;
        runnerBorder.enabled = false;

        if(pursuerBorder.enabled)
        {
            playerChosen = AvailablePlayers.Pursuer;
            specificObjective.text = "Catch the Runner.";
            playButtonText.text = "Play as Pursuer";
            SetHumanPlayer(isRunnerHuman: false, isPursuerHuman: true);
        }
        else
        {
            playerChosen = AvailablePlayers.None;
            playButtonText.text = "Spectate";
            specificObjective.text = "";

            SetHumanPlayer(isRunnerHuman: false, isPursuerHuman: false);
        }
    }

    public void Play()
    {
        SceneManager.LoadScene(sceneBuildIndex: 1);
    }
}
