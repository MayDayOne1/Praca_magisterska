using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button runnerButton;
    [SerializeField] private Button pursuerButton;
    [SerializeField] private Image runnerBorder;
    [SerializeField] private Image pursuerBorder;
    [SerializeField] private Button playButton;
    [SerializeField] private TextMeshProUGUI specificObjective;

    public enum AvailablePlayers
    {
        None,
        Runner,
        Pursuer
    }

    public AvailablePlayers playerChosen = AvailablePlayers.None;

    public void ToggleRunner()
    {
        runnerBorder.enabled = !runnerBorder.enabled;
        pursuerBorder.enabled = false;

        if (runnerBorder.enabled)
        {
            playerChosen = AvailablePlayers.Runner;
            playButton.interactable = true;
            specificObjective.text = "Get to the goal, avoid the Pursuer.";
        }
        else
        {
            playerChosen = AvailablePlayers.None;
            playButton.interactable = false;
            specificObjective.text = "";
        }

    }

    public void TogglePursuer()
    {
        pursuerBorder.enabled = !pursuerBorder.enabled;
        runnerBorder.enabled = false;

        if(pursuerBorder.enabled)
        {
            playerChosen = AvailablePlayers.Pursuer;
            playButton.interactable = true;
            specificObjective.text = "Catch the Runner.";
        }
        else
        {
            playerChosen = AvailablePlayers.None;
            playButton.interactable = false;
            specificObjective.text = "";
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("MainScene");
    }
}
