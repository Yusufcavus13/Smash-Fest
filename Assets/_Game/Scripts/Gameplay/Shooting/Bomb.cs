using System.Collections;
using SmashFest.Gameplay.Objects;
using SmashFest.Interactions;
using SmashFest.Systems;
using UnityEngine;

namespace SmashFest.Gameplay.Shooting
{
    /// <summary>
    /// The booster projectile. Unlike the plain ball it does not rely on the hit to do the
    /// work: it flies to the point the player picked and detonates there, throwing a burst of
    /// force at everything nearby so a whole cluster topples at once.
    /// </summary>
    [DisallowMultipleComponent]
    public class Bomb : MonoBehaviour
    {
        // --- Serialized Fields ---

        [Header("References")]
        [SerializeField] private Rigidbody body;
        [SerializeField] private Transform marker;

        [Header("Explosion")]
        [Tooltip("How far the blast reaches.")]
        [SerializeField] private float radius = 3.2f;

        [Tooltip("Outward shove given to each caught body.")]
        [SerializeField] private float force = 14f;

        [Tooltip("Lifts the blast so bodies tip over rather than just slide.")]
        [SerializeField] private float upwardModifier = 1.4f;

        [Tooltip("Damage dealt at the centre, tapering to zero at the rim.")]
        [SerializeField] private float centreDamage = 30f;

        [Tooltip("Layers the blast is allowed to move.")]
        [SerializeField] private LayerMask affectLayers;

        [Header("Feel")]
        [SerializeField] private string blastEffectId = "fx_blast";
        [SerializeField] private float armDelay = 0.04f;

        [Header("Flight")]
        [Tooltip("Seconds before an untouched bomb gives up and detonates in the air.")]
        [SerializeField] private float maxFlightTime = 2.5f;

        // --- Fields ---

        private Vector3 targetPoint;
        private bool hasTarget;
        private bool exploded;
        private float armTime;
        private Coroutine flightRoutine;
        private Vector3 markerLocalPosition;
        private Quaternion markerLocalRotation;
        private bool markerCached;

        // --- Unity Messages ---

        private void OnEnable()
        {
            CacheMarker();
            exploded = false;
            hasTarget = false;
            body.isKinematic = false;
            body.useGravity = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            if (marker != null)
            {
                marker.gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            if (flightRoutine != null)
            {
                StopCoroutine(flightRoutine);
                flightRoutine = null;
            }

            RestoreMarker();
        }

        private void CacheMarker()
        {
            if (markerCached || marker == null)
            {
                return;
            }

            markerLocalPosition = marker.localPosition;
            markerLocalRotation = marker.localRotation;
            markerCached = true;
        }

        // Launch detaches the marker so it stays at the target while the bomb flies, so it
        // has to be pulled back into the prefab before this returns to the pool.
        private void RestoreMarker()
        {
            if (marker == null)
            {
                return;
            }

            marker.SetParent(transform, false);
            marker.localPosition = markerLocalPosition;
            marker.localRotation = markerLocalRotation;
            marker.gameObject.SetActive(false);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (exploded || Time.time < armTime)
            {
                return;
            }

            Explode(collision.GetContact(0).point);
        }

        // --- Public Methods ---

        /// <summary>
        /// Sends the bomb toward <paramref name="target"/>. A marker is dropped there so the
        /// player can see where the blast is going before it lands.
        /// </summary>
        public void Launch(Vector3 velocity, Vector3 target)
        {
            targetPoint = target;
            hasTarget = true;
            armTime = Time.time + armDelay;
            body.linearVelocity = velocity;

            if (marker != null)
            {
                marker.SetParent(null, true);
                marker.position = target;
                marker.gameObject.SetActive(true);
            }

            flightRoutine = StartCoroutine(FlightRoutine());
        }

        // --- Private Methods ---

        private IEnumerator FlightRoutine()
        {
            float elapsed = 0f;

            while (!exploded)
            {
                elapsed += Time.deltaTime;

                // Detonate the moment the bomb reaches the aimed point, even in mid air.
                if (hasTarget && Time.time >= armTime)
                {
                    Vector3 flat = transform.position - targetPoint;
                    flat.y = 0f;
                    if (flat.sqrMagnitude < 0.16f)
                    {
                        Explode(transform.position);
                        yield break;
                    }
                }

                if (elapsed >= maxFlightTime)
                {
                    Explode(transform.position);
                    yield break;
                }

                yield return null;
            }
        }

        private void Explode(Vector3 point)
        {
            if (exploded)
            {
                return;
            }

            exploded = true;

            if (marker != null)
            {
                marker.gameObject.SetActive(false);
            }

            Collider[] caught = Physics.OverlapSphere(point, radius, affectLayers, QueryTriggerInteraction.Ignore);
            var pushed = new System.Collections.Generic.HashSet<Rigidbody>();

            for (int i = 0; i < caught.Length; i++)
            {
                Rigidbody rb = caught[i].attachedRigidbody;
                if (rb == null || rb == body || !pushed.Add(rb))
                {
                    continue;
                }

                rb.AddExplosionForce(force, point, radius, upwardModifier, ForceMode.Impulse);

                if (caught[i].TryGetComponent(out LevelObject levelObject))
                {
                    float t = 1f - Mathf.Clamp01(Vector3.Distance(point, rb.worldCenterOfMass) / radius);
                    SmashHitInfo hit = new SmashHitInfo(point, Vector3.up, centreDamage * t);
                    levelObject.TakeHit(hit);
                }
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShatter(SmashFest.Core.MaterialType.Stone);
            }
            Haptics.Play();

            if (!string.IsNullOrEmpty(blastEffectId) && PoolManager.Instance != null)
            {
                PoolManager.Instance.Spawn(blastEffectId, point, Quaternion.identity);
            }

            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
