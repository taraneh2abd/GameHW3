using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleRelease : MonoBehaviour
{
    private void OnParticleSystemStopped()
    {
        PooledObject pooled = GetComponent<PooledObject>();
        if (pooled != null)
        {
            pooled.Release();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
