using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileStrike : MonoBehaviour
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private GameObject blastPrefab;
    [SerializeField] private float flightTime = 0.4f;

    private void OnEnable()
    {
        GameEvents.MissileLaunched += HandleMissileLaunched;
    }

    private void OnDisable()
    {
        GameEvents.MissileLaunched -= HandleMissileLaunched;
    }

    private void HandleMissileLaunched()
    {
        StartCoroutine(StrikeRoutine());
    }

    private IEnumerator StrikeRoutine()
    {
        Camera cam = Camera.main;
        Vector3 target = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
        Vector3 start = StartPosition(cam);

        GameObject missile = Instantiate(missilePrefab, start, Quaternion.identity);
        Vector3 direction = target - start;
        if (direction.sqrMagnitude > 0.01f)
        {
            missile.transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        }

        float elapsed = 0f;
        while (elapsed < flightTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flightTime);
            missile.transform.position = Vector3.Lerp(start, target, t * t);
            yield return null;
        }

        Destroy(missile);

        GameObject blast = Instantiate(blastPrefab, target, Quaternion.identity);
        GameEvents.RaiseMissileDetonated(target);
        ClearScreen();

        SpriteAnimator animator = blast.GetComponent<SpriteAnimator>();
        while (animator != null && !animator.Finished)
        {
            yield return null;
        }

        Destroy(blast);
    }

    private Vector3 StartPosition(Camera cam)
    {
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if (player != null)
        {
            return player.transform.position;
        }

        return new Vector3(cam.transform.position.x, cam.transform.position.y - cam.orthographicSize, 0f);
    }

    private void ClearScreen()
    {
        List<IScreenClearTarget> targets = new List<IScreenClearTarget>();

        foreach (MonoBehaviour behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (behaviour is IScreenClearTarget target)
            {
                targets.Add(target);
            }
        }

        foreach (IScreenClearTarget target in targets)
        {
            target.OnScreenClear();
        }
    }
}
