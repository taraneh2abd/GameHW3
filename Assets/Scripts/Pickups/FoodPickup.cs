using UnityEngine;

public class FoodPickup : Pickup
{
    [SerializeField] private int scoreValue = 25;
    [SerializeField] private int foodValue = 1;

    public int ScoreValue => scoreValue;
    public int FoodValue => foodValue;

    protected override void Collect()
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.AddScore(scoreValue);
        GameManager.Instance.AddFood(foodValue);
    }
}
