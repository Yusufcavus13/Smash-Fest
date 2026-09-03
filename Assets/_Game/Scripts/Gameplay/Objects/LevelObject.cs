using System;
using System.Collections;
using SmashFest.Core;
using SmashFest.Interactions;
using SmashFest.Systems;
using UnityEngine;

namespace SmashFest.Gameplay.Objects
{

    [DisallowMultipleComponent]
    public class LevelObject : MonoBehaviour, ISmashable
    {

        public static event Action<LevelObject> Cleared;


        public bool IsSmashable => !isBroken;
        public bool IsCleared => isCleared;
        public Rigidbody Body => body;


        [Header("References")]
        [SerializeField] protected Rigidbody body;
        [SerializeField] protected Collider bodyCollider;
        [SerializeField] protected MeshRenderer meshRenderer;

        [Tooltip("Optional: an imported model to hide on break instead of meshRenderer. "
            + "Lets multi-mesh models stand in for the plain body.")]
        [SerializeField] protected GameObject visualRoot;
        [SerializeField] protected GameObject fracturedRoot;
        [SerializeField] protected Rigidbody[] fracturePieces;

        [Header("Identity")]
        [Tooltip("Decides how this sounds when it is hit and when it breaks.")]
        [SerializeField] protected MaterialType materialType = MaterialType.Wood;

        [Header("Durability")]
        [SerializeField] protected float maxHealth = 10f;
        [SerializeField] protected float impulseThreshold = 2f;

        [Tooltip("Damage taken per impulse from balls and other objects.")]
        [SerializeField] protected float damagePerImpulse = 1f;

        [Tooltip("Damage taken per impulse when landing on the ground. Usually much higher.")]
        [SerializeField] protected float groundDamagePerImpulse = 3f;

        [SerializeField] protected LayerMask groundLayers;

        [Header("Impact Effect")]
        [Tooltip("Pool id of the dust puff spawned when this lands on the ground.")]
        [SerializeField] protected string groundImpactEffectId = "fx_dust";

        [Tooltip("Pool id of the puff spawned when the ball or another object hits this.")]
        [SerializeField] protected string hitEffectId = "fx_hit";

        [Tooltip("Minimum impulse needed before the ground puff is spawned.")]
        [SerializeField] protected float effectImpulseThreshold = 4f;

        [Header("Fracture")]
        [Tooltip("Scatter speed in m/s on the weakest possible break.")]
        [SerializeField] protected float baseScatterSpeed = 0.4f;

        [Tooltip("Extra scatter speed per unit of impact impulse.")]
        [SerializeField] protected float scatterSpeedPerImpulse = 0.06f;

        [Tooltip("Upper limit of the scatter speed, however hard the hit was.")]
        [SerializeField] protected float maxScatterSpeed = 2.5f;

        [Tooltip("How much of the object's own velocity the pieces keep.")]
        [Range(0f, 1f)]
        [SerializeField] protected float velocityInheritance = 0.6f;

        [SerializeField] protected float fractureRadius = 1.5f;
        [SerializeField] protected float fractureUpwardModifier = 0.1f;
        [SerializeField] protected float pieceSpin = 2.5f;
        [SerializeField] protected float despawnDelay = 5f;

        [Header("Feedback")]
        [Tooltip("Impulse at which an impact plays at full volume.")]
        [SerializeField] protected float loudImpulse = 12f;

        [Tooltip("Only breaks at least this hard are worth a buzz.")]
        [SerializeField] protected float hapticImpulseThreshold = 8f;


        [Header("Out of bounds")]
        [Tooltip("Once knocked past these world limits the block is counted as cleared, then left "
            + "to keep tumbling for despawnDelay before it fades, so debris lingers a moment.")]
        [SerializeField] protected float boundsMinZ = -4.5f;
        [SerializeField] protected float boundsMaxZ = 4.5f;
        [SerializeField] protected float boundsMaxX = 6f;
        [SerializeField] protected float boundsMinY = -3f;

        [Tooltip("Hard limit toward the camera: a block this close to the lens is removed at once "
            + "(no lingering), so nothing can ever fill the screen. Camera sits near z = -12.5.")]
        [SerializeField] protected float cameraGuardZ = -8f;

        protected bool isCleared;
        protected bool isBroken;

        private Coroutine despawnRoutine;
        private float currentHealth;
        private Vector3[] pieceLocalPositions;
        private Quaternion[] pieceLocalRotations;
        private WaitForSeconds despawnWait;


        protected virtual void Awake()
        {
            despawnWait = new WaitForSeconds(despawnDelay);
            CacheFractureLayout();
        }

        protected virtual void OnEnable()
        {
            ResetObject();
        }

        private void FixedUpdate()
        {
            Vector3 p = transform.position;

            // Lens guard runs even after the block is cleared: a piece drifting this close to the
            // camera is yanked instantly so it can never fill the screen.
            if (p.z < cameraGuardZ)
            {
                HardRemove();
                return;
            }

            if (isBroken || isCleared)
            {
                return;
            }

            // Shoved off the table (sideways, back, below, or a way toward the camera): count it
            // now, but leave it visible to keep tumbling for despawnDelay before it fades.
            if (p.z < boundsMinZ || p.z > boundsMaxZ
                || p.x < -boundsMaxX || p.x > boundsMaxX
                || p.y < boundsMinY)
            {
                MarkClearedOutOfBounds();
            }
        }

        private void MarkClearedOutOfBounds()
        {
            if (!isCleared)
            {
                isCleared = true;
                Cleared?.Invoke(this);
            }

            // Left visible and still driven by physics; it tumbles off, then despawns on the timer.
            ScheduleDespawn(true);
        }

        private void HardRemove()
        {
            if (isBroken && !gameObject.activeSelf)
            {
                return;
            }

            meshRenderer.enabled = false;
            if (visualRoot != null)
            {
                visualRoot.SetActive(false);
            }

            bodyCollider.enabled = false;
            body.isKinematic = true;

            if (!isCleared)
            {
                isCleared = true;
                Cleared?.Invoke(this);
            }

            Despawn();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (isBroken)
            {
                return;
            }

            float impulse = collision.impulse.magnitude;
            if (impulse < impulseThreshold)
            {
                return;
            }

            ContactPoint contact = collision.GetContact(0);
            SmashHitInfo hitInfo = new SmashHitInfo(contact.point, -contact.normal, impulse);

            bool groundImpact = IsGroundLayer(collision.gameObject.layer);

            if (impulse >= effectImpulseThreshold)
            {
                SpawnImpactEffect(groundImpact ? groundImpactEffectId : hitEffectId,
                    contact.point, contact.normal);
            }

            float multiplier = groundImpact ? groundDamagePerImpulse : damagePerImpulse;

            ApplyDamage(impulse * multiplier, hitInfo);
        }


        public virtual void TakeHit(in SmashHitInfo hitInfo)
        {
            ApplyDamage(hitInfo.Impulse * damagePerImpulse, hitInfo);
        }

        public void MarkCleared()
        {
            if (isCleared)
            {
                return;
            }

            isCleared = true;
            Cleared?.Invoke(this);
            ScheduleDespawn(false);
        }
        public virtual void ResetObject()
        {
            isCleared = false;
            isBroken = false;
            despawnRoutine = null;
            currentHealth = maxHealth;

            body.isKinematic = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            bodyCollider.enabled = true;
            if (visualRoot != null)
            {
                // An imported model stands in for the body; keep the plain mesh hidden.
                visualRoot.SetActive(true);
                meshRenderer.enabled = false;
            }
            else
            {
                meshRenderer.enabled = true;
            }

            for (int i = 0; i < fracturePieces.Length; i++)
            {
                Rigidbody piece = fracturePieces[i];
                piece.isKinematic = true;
                piece.transform.SetLocalPositionAndRotation(pieceLocalPositions[i], pieceLocalRotations[i]);
            }

            fracturedRoot.SetActive(false);
        }


        protected void ApplyDamage(float amount, in SmashHitInfo hitInfo)
        {
            if (isBroken)
            {
                return;
            }

            currentHealth -= amount;

            if (currentHealth <= 0f)
            {
                Break(hitInfo);
                return;
            }

            OnDamaged(hitInfo);
        }

        protected virtual void OnDamaged(in SmashHitInfo hitInfo)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayImpact(materialType, hitInfo.Impulse / loudImpulse);
            }
        }

        protected virtual void OnBroken(in SmashHitInfo hitInfo)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShatter(materialType);
            }

            // Every crate buzzing would be unbearable, so only a solid hit gets through.
            if (hitInfo.Impulse >= hapticImpulseThreshold)
            {
                Haptics.Play();
            }
        }

        protected virtual void Break(in SmashHitInfo hitInfo)
        {
            isBroken = true;

            Vector3 inheritedVelocity = body.linearVelocity * velocityInheritance;

            meshRenderer.enabled = false;
            if (visualRoot != null)
            {
                visualRoot.SetActive(false);
            }
            bodyCollider.enabled = false;
            body.isKinematic = true;

            SpawnFracture(hitInfo, inheritedVelocity);
            OnBroken(hitInfo);

            if (!isCleared)
            {
                isCleared = true;
                Cleared?.Invoke(this);
            }

            ScheduleDespawn(true);
        }

        protected virtual void SpawnFracture(in SmashHitInfo hitInfo, Vector3 inheritedVelocity)
        {
            fracturedRoot.SetActive(true);

            float scatterSpeed = Mathf.Min(
                baseScatterSpeed + hitInfo.Impulse * scatterSpeedPerImpulse,
                maxScatterSpeed);

            for (int i = 0; i < fracturePieces.Length; i++)
            {
                Rigidbody piece = fracturePieces[i];
                piece.isKinematic = false;
                piece.linearVelocity = inheritedVelocity;
                piece.angularVelocity = UnityEngine.Random.insideUnitSphere * pieceSpin;
                piece.AddExplosionForce(
                    scatterSpeed,
                    hitInfo.Point,
                    fractureRadius,
                    fractureUpwardModifier,
                    ForceMode.VelocityChange);
            }
        }

        protected virtual void Despawn()
        {
            PoolManager.Instance.Despawn(gameObject);
        }


        /// <summary>
        /// The puff at the contact point. The effect is aimed along the contact normal so it
        /// sprays away from the surface that was hit.
        /// </summary>
        private void SpawnImpactEffect(string effectId, Vector3 point, Vector3 normal)
        {
            if (string.IsNullOrEmpty(effectId))
            {
                return;
            }

            PoolManager.Instance.Spawn(effectId, point, Quaternion.LookRotation(normal));
        }

        private bool IsGroundLayer(int layer)
        {
            return (groundLayers.value & (1 << layer)) != 0;
        }

        private void ScheduleDespawn(bool restart)
        {
            if (despawnRoutine != null)
            {
                if (!restart)
                {
                    return;
                }

                StopCoroutine(despawnRoutine);
            }

            despawnRoutine = StartCoroutine(DespawnRoutine());
        }

        private IEnumerator DespawnRoutine()
        {
            yield return despawnWait;
            Despawn();
        }

        private void CacheFractureLayout()
        {
            pieceLocalPositions = new Vector3[fracturePieces.Length];
            pieceLocalRotations = new Quaternion[fracturePieces.Length];

            for (int i = 0; i < fracturePieces.Length; i++)
            {
                Transform pieceTransform = fracturePieces[i].transform;
                pieceLocalPositions[i] = pieceTransform.localPosition;
                pieceLocalRotations[i] = pieceTransform.localRotation;
            }
        }

#if UNITY_EDITOR
        // Editor only helper: fills the fracture list while the designer builds the prefab.
        protected virtual void OnValidate()
        {
            if (fracturedRoot == null)
            {
                return;
            }

            if (fracturePieces == null || fracturePieces.Length == 0)
            {
                fracturePieces = fracturedRoot.GetComponentsInChildren<Rigidbody>(true);
            }
        }
#endif
    }
}
