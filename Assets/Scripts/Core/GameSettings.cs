using System;
using UnityEngine;

public static class GameSettings
{
    public const float DefaultMusicVolume = 0.7f;
    public const float DefaultSfxVolume = 0.8f;

    private const string MusicKey = "settings.music";
    private const string SfxKey = "settings.sfx";

    public static event Action Changed;

    public static float MusicVolume
    {
        get => PlayerPrefs.GetFloat(MusicKey, DefaultMusicVolume);
        set
        {
            PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxKey, DefaultSfxVolume);
        set
        {
            PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value));
            PlayerPrefs.Save();
            Changed?.Invoke();
        }
    }
}
