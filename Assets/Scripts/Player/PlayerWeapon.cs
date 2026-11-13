using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;
    [SerializeField] private WeaponDataSO[] weaponsByLevel;
    [SerializeField] private Transform muzzle;
    [SerializeField] private float parallelOffset = 0.18f;
    [SerializeField] private float spreadAngle = 15f;

    private int powerLevel = 1;
    private float nextFireTime;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            powerLevel = GameManager.Instance.PowerLevel;
        }

        GameEvents.PowerLevelChanged += HandlePowerLevelChanged;
        inputReader.MissilePressed += HandleMissilePressed;
    }

    private void OnDisable()
    {
        GameEvents.PowerLevelChanged -= HandlePowerLevelChanged;
        inputReader.MissilePressed -= HandleMissilePressed;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f || !inputReader.FireHeld || Time.time < nextFireTime)
        {
            return;
        }

        WeaponDataSO weapon = CurrentWeapon();
        if (weapon == null || PoolManager.Instance == null)
        {
            return;
        }

        Fire(weapon);
        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, weapon.FireRate);
    }

    private WeaponDataSO CurrentWeapon()
    {
        if (weaponsByLevel == null || weaponsByLevel.Length == 0)
        {
            return null;
        }

        int index = Mathf.Clamp(powerLevel, 1, weaponsByLevel.Length) - 1;
        return weaponsByLevel[index];
    }

    private void Fire(WeaponDataSO weapon)
    {
        Vector3 origin = muzzle.position;
        Quaternion baseRotation = muzzle.rotation;

        switch (weapon.Pattern)
        {
            case FirePattern.Single:
                Spawn(weapon, origin, baseRotation);
                break;
            case FirePattern.DoubleParallel:
                Vector3 side = muzzle.right * parallelOffset;
                Spawn(weapon, origin - side, baseRotation);
                Spawn(weapon, origin + side, baseRotation);
                break;
            case FirePattern.Spread:
                Spawn(weapon, origin, baseRotation * Quaternion.Euler(0f, 0f, spreadAngle));
                Spawn(weapon, origin, baseRotation);
                Spawn(weapon, origin, baseRotation * Quaternion.Euler(0f, 0f, -spreadAngle));
                break;
        }
    }

    private void Spawn(WeaponDataSO weapon, Vector3 position, Quaternion rotation)
    {
        GameObject instance = PoolManager.Instance.Get(weapon.BulletPrefab, position, rotation);
        Bullet bullet = instance.GetComponent<Bullet>();
        bullet.Launch(weapon.Damage, weapon.BulletSpeed);
    }

    private void HandlePowerLevelChanged(int level)
    {
        powerLevel = level;
    }

    private void HandleMissilePressed()
    {
        if (Time.timeScale <= 0f)
        {
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.TryUseMissile())
        {
            GameEvents.RaiseMissileLaunched();
        }
    }
}
