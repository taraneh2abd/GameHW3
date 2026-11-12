using System;
using UnityEngine;

[Serializable]
public class LevelTheme
{
    [SerializeField] private string displayName;
    [SerializeField] private Color backgroundTint = Color.white;
    [SerializeField] private Sprite[] parallaxLayers;

    public string DisplayName => displayName;
    public Color BackgroundTint => backgroundTint;
    public Sprite[] ParallaxLayers => parallaxLayers;
}
