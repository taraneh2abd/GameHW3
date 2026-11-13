using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Awake()
    {
        newGameButton.onClick.AddListener(StartNewGame);
        continueButton.onClick.AddListener(ContinueSavedGame);
        quitButton.onClick.AddListener(QuitGame);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSfxVolume);
    }

    private void OnEnable()
    {
        continueButton.gameObject.SetActive(SaveSystem.HasSave());
        musicSlider.SetValueWithoutNotify(GameSettings.MusicVolume);
        sfxSlider.SetValueWithoutNotify(GameSettings.SfxVolume);
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

    private void SetMusicVolume(float value)
    {
        GameSettings.MusicVolume = value;
    }

    private void SetSfxVolume(float value)
    {
        GameSettings.SfxVolume = value;
    }
}
