using System.Collections;
using SmashFest.Systems;
using UnityEngine;

namespace SmashFest.Gameplay.Shooting
{
    
    [DisallowMultipleComponent]
    public class Ball : MonoBehaviour
    {
       

        [Header("References")]
        [SerializeField] private Rigidbody body;

        [Header("Flight")]
        [Tooltip("Seconds the ball stays in the scene before it returns to the pool.")]
        [SerializeField] private float lifetime = 2f;

      

        private WaitForSeconds lifetimeWait;
        private Coroutine lifetimeRoutine;

        

        private void Awake()
        {
            lifetimeWait = new WaitForSeconds(lifetime);
        }

        private void OnEnable()
        {
            body.isKinematic = false;
            body.useGravity = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;

            lifetimeRoutine = StartCoroutine(LifetimeRoutine());
        }

        private void OnDisable()
        {
            if (lifetimeRoutine != null)
            {
                StopCoroutine(lifetimeRoutine);
                lifetimeRoutine = null;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            body.useGravity = true;
        }


        public void Launch(Vector3 velocity)
        {
            body.linearVelocity = velocity;
        }


        private IEnumerator LifetimeRoutine()
        {
            yield return lifetimeWait;
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
