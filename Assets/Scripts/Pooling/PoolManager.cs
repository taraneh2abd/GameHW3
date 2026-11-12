using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private readonly Dictionary<GameObject, ObjectPool<GameObject>> pools = new Dictionary<GameObject, ObjectPool<GameObject>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        GameObject instance = GetPool(prefab).Get();
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
        {
            return;
        }

        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled == null || pooled.Prefab == null || !pools.TryGetValue(pooled.Prefab, out ObjectPool<GameObject> pool))
        {
            Destroy(instance);
            return;
        }

        if (pooled.InPool)
        {
            return;
        }

        pool.Release(instance);
    }

    public void Prewarm(GameObject prefab, int count)
    {
        ObjectPool<GameObject> pool = GetPool(prefab);
        List<GameObject> created = new List<GameObject>(count);

        for (int i = 0; i < count; i++)
        {
            created.Add(pool.Get());
        }

        foreach (GameObject instance in created)
        {
            pool.Release(instance);
        }
    }

    private ObjectPool<GameObject> GetPool(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out ObjectPool<GameObject> pool))
        {
            pool = new ObjectPool<GameObject>(
                () => CreateInstance(prefab),
                OnGet,
                OnRelease,
                obj => Destroy(obj),
                true,
                16,
                1000);
            pools.Add(prefab, pool);
        }

        return pool;
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, transform);
        instance.SetActive(false);

        PooledObject pooled = instance.GetComponent<PooledObject>();
        if (pooled == null)
        {
            pooled = instance.AddComponent<PooledObject>();
        }

        pooled.Prefab = prefab;
        return instance;
    }

    private void OnGet(GameObject instance)
    {
        instance.GetComponent<PooledObject>().InPool = false;
    }

    private void OnRelease(GameObject instance)
    {
        instance.GetComponent<PooledObject>().InPool = true;
        instance.SetActive(false);
        instance.transform.SetParent(transform);
    }
}
