using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private Text scoreText;
    [SerializeField] private Image[] heartIcons;
    [SerializeField] private Text missileText;
    [SerializeField] private RectTransform foodFill;

    private void OnEnable()
    {
        GameEvents.RoundStarted += HandleRoundStarted;
        GameEvents.ScoreChanged += HandleScoreChanged;
        GameEvents.LivesChanged += HandleLivesChanged;
        GameEvents.MissilesChanged += HandleMissilesChanged;
        GameEvents.FoodMeterChanged += HandleFoodChanged;
    }

    private void OnDisable()
    {
        GameEvents.RoundStarted -= HandleRoundStarted;
        GameEvents.ScoreChanged -= HandleScoreChanged;
        GameEvents.LivesChanged -= HandleLivesChanged;
        GameEvents.MissilesChanged -= HandleMissilesChanged;
        GameEvents.FoodMeterChanged -= HandleFoodChanged;
    }

    private void Start()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null)
        {
            return;
        }

        LevelDataSO level = manager.CurrentLevel;
        int levelNumber = level != null ? level.LevelNumber : manager.LevelIndex + 1;
        HandleRoundStarted(levelNumber, manager.RoundIndex + 1);
        manager.BroadcastState();
    }

    private void HandleRoundStarted(int levelNumber, int roundNumber)
    {
        levelText.text = "LEVEL " + levelNumber + " - ROUND " + roundNumber;
    }

    private void HandleScoreChanged(int score)
    {
        scoreText.text = "SCORE " + score.ToString("D6");
    }

    private void HandleLivesChanged(int lives)
    {
        for (int i = 0; i < heartIcons.Length; i++)
        {
            heartIcons[i].enabled = i < lives;
        }
    }

    private void HandleMissilesChanged(int missiles)
    {
        missileText.text = "x " + missiles;
    }

    private void HandleFoodChanged(float normalized)
    {
        SetFill(foodFill, normalized);
    }

    private static void SetFill(RectTransform fill, float amount)
    {
        fill.anchorMax = new Vector2(Mathf.Clamp01(amount), 1f);
    }
}
