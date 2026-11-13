using System;
using UnityEngine;

[Serializable]
public class LevelTheme
{
    [SerializeField] private string displayName;
    [SerializeField] private GameObject backgroundPrefab;

    public string DisplayName => displayName;
    public GameObject BackgroundPrefab => backgroundPrefab;
}