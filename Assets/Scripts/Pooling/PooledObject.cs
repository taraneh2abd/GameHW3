using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject Prefab { get; set; }
    public bool InPool { get; set; }

    public void Release()
    {
        if (PoolManager.Instance != null)
        {
            PoolManager.Instance.Release(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
