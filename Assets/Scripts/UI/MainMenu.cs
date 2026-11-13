using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueSavedGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    private void OnEnable()
    {
        continueButton.gameObject.SetActive(SaveSystem.HasSave());
    }

    private void Start()
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(newGameButton.gameObject);
        }
    }

    private void StartNewGame()
    {
        GameManager.Instance.NewGame();
    }

    private void ContinueSavedGame()
    {
        GameManager.Instance.ContinueGame();
    }

    private void QuitGame()
    {
        Application.Quit();
    }
}
