using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Game/Weapon Data")]
public class WeaponDataSO : ScriptableObject
{
    [SerializeField] private string weaponName;
    [SerializeField] private Sprite sprite;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int damage = 1;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float fireRate = 4f;
    [SerializeField] private FirePattern pattern;

    public string WeaponName => weaponName;
    public Sprite Sprite => sprite;
    public GameObject BulletPrefab => bulletPrefab;
    public int Damage => damage;
    public float BulletSpeed => bulletSpeed;
    public float FireRate => fireRate;
    public FirePattern Pattern => pattern;
}
