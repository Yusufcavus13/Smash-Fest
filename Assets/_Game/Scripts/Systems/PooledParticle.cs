using System.Collections;
using UnityEngine;

namespace SmashFest.Systems
{
    /// <summary>
    /// A one shot particle effect that lives in the pool. It plays itself on spawn and
    /// returns itself when the burst is over, so callers only have to ask for it.
    /// </summary>
    [DisallowMultipleComponent]
    public class PooledParticle : MonoBehaviour
    {
        // --- Serialized Fields ---

        [Header("References")]
        [SerializeField] private ParticleSystem particles;

        [Header("Timing")]
        [Tooltip("Seconds before the effect returns to the pool.")]
        [SerializeField] private float lifetime = 1.5f;

        // --- Fields ---

        private WaitForSeconds lifetimeWait;

        // --- Unity Messages ---

        private void Awake()
        {
            lifetimeWait = new WaitForSeconds(lifetime);
        }

        private void OnEnable()
        {
            particles.Clear();
            particles.Play();

            StartCoroutine(DespawnRoutine());
        }

        // --- Private Methods ---

        private IEnumerator DespawnRoutine()
        {
            yield return lifetimeWait;
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
