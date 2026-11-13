using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EndScreens : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverScoreText;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button gameOverMenuButton;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private Text victoryScoreText;
    [SerializeField] private Button victoryMenuButton;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Text levelCompleteText;

    private int lastScore;

    private void Awake()
    {
        retryButton.onClick.AddListener(Retry);
        gameOverMenuButton.onClick.AddListener(ReturnToMenu);
        victoryMenuButton.onClick.AddListener(ReturnToMenu);
        gameOverPanel.SetActive(false);
        victoryPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.ScoreChanged += HandleScoreChanged;
        GameEvents.GameOver += HandleGameOver;
        GameEvents.Victory += HandleVictory;
        GameEvents.LevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        GameEvents.ScoreChanged -= HandleScoreChanged;
        GameEvents.GameOver -= HandleGameOver;
        GameEvents.Victory -= HandleVictory;
        GameEvents.LevelCompleted -= HandleLevelCompleted;
    }

    private void HandleScoreChanged(int score)
    {
        lastScore = score;
    }

    private void HandleGameOver()
    {
        levelCompletePanel.SetActive(false);
        gameOverScoreText.text = "SCORE " + lastScore.ToString("D6");
        gameOverPanel.SetActive(true);
        Select(retryButton);
    }

    private void HandleVictory()
    {
        levelCompletePanel.SetActive(false);
        victoryScoreText.text = "SCORE " + lastScore.ToString("D6");
        victoryPanel.SetActive(true);
        Select(victoryMenuButton);
    }

    private void HandleLevelCompleted(int levelNumber)
    {
        levelCompleteText.text = "LEVEL " + levelNumber + " COMPLETE";
        levelCompletePanel.SetActive(true);
    }

    private void Retry()
    {
        gameOverPanel.SetActive(false);
        GameManager.Instance.Retry();
    }

    private void ReturnToMenu()
    {
        GameManager.Instance.ReturnToMenu();
    }

    private static void Select(Button button)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(button.gameObject);
        }
    }
}
