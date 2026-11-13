using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private GameObject panel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button menuButton;

    private bool paused;
    private bool blocked;

    public bool IsPaused => paused;

    private void Awake()
    {
        resumeButton.onClick.AddListener(Resume);
        menuButton.onClick.AddListener(ReturnToMenu);
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        inputReader.PausePressed += HandlePausePressed;
        GameEvents.GameOver += Block;
        GameEvents.Victory += Block;
        GameEvents.LevelCompleted += HandleLevelCompleted;
    }

    private void OnDisable()
    {
        inputReader.PausePressed -= HandlePausePressed;
        GameEvents.GameOver -= Block;
        GameEvents.Victory -= Block;
        GameEvents.LevelCompleted -= HandleLevelCompleted;

        if (paused)
        {
            Resume();
        }
    }

    private void HandlePausePressed()
    {
        if (paused)
        {
            Resume();
        }
        else
        {
            Pause();
        }
    }

    private void Pause()
    {
        if (blocked)
        {
            return;
        }

        paused = true;
        Time.timeScale = 0f;
        panel.SetActive(true);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    private void Resume()
    {
        paused = false;
        Time.timeScale = 1f;
        panel.SetActive(false);
    }

    private void ReturnToMenu()
    {
        Resume();
        GameManager.Instance.ReturnToMenu();
    }

    private void Block()
    {
        blocked = true;
    }

    private void HandleLevelCompleted(int levelNumber)
    {
        blocked = true;
    }
}
