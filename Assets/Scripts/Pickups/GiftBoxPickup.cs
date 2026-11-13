using UnityEngine;

public class GiftBoxPickup : Pickup
{
    [SerializeField] private int maxLevelBonus = 100;

    public int MaxLevelBonus => maxLevelBonus;

    protected override void Collect()
    {
        GameManager manager = GameManager.Instance;
        if (manager == null)
        {
            return;
        }

        int before = manager.PowerLevel;
        manager.UpgradeWeapon();

        if (manager.PowerLevel == before)
        {
            manager.AddScore(maxLevelBonus);
        }
    }
}
