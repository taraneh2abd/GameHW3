using UnityEngine;

[CreateAssetMenu(fileName = "NewLevel", menuName = "Game/Level Data")]
public class LevelDataSO : ScriptableObject
{
    [SerializeField] private int levelNumber;
    [SerializeField] private string sceneName;
    [SerializeField] private AudioClip music;
    [SerializeField] private LevelTheme theme;
    [SerializeField] private RoundData[] rounds;
    [SerializeField] private LevelDataSO nextLevel;

    public int LevelNumber => levelNumber;
    public string SceneName => sceneName;
    public AudioClip Music => music;
    public LevelTheme Theme => theme;
    public RoundData[] Rounds => rounds;
    public LevelDataSO NextLevel => nextLevel;
}
