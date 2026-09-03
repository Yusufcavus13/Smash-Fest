using SmashFest.Core;
using SmashFest.Levels;
using SmashFest.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace SmashFest.Gameplay.Shooting
{
    public class CannonController : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Transform muzzle;

        [Tooltip("The barrel pivot that swings to face where the player shoots.")]
        [SerializeField] private Transform aimPivot;

        [Tooltip("Shown only while a level is being played; hidden on Home and the end panels. "
            + "Defaults to the aim pivot when left empty.")]
        [SerializeField] private GameObject visualRoot;

        [Tooltip("Flash pooled at the muzzle each time the cannon fires. Leave empty to skip.")]
        [SerializeField] private string muzzleFlashId = "fx_muzzle";

        [Header("Shooting")]
        [SerializeField] private string ballPoolId = "ball";
        [SerializeField] private float ballSpeed = 25f;

        [Header("Bomb Booster")]
        [SerializeField] private string bombPoolId = "bomb";
        [Tooltip("Bombs travel slower than balls so the marker is readable on the way in.")]
        [SerializeField] private float bombSpeed = 16f;

        [Tooltip("How far the aim ray reaches when the player taps empty space.")]
        [SerializeField] private float maxAimDistance = 60f;

        [SerializeField] private LayerMask aimLayers;


        /// <summary>Raised the moment an armed bomb is actually fired, so the booster
        /// button can return to its idle state.</summary>
        public static event System.Action BombFired;

        private bool bombArmed;

        public bool BombArmed => bombArmed;

        /// <summary>
        /// Arms the bomb booster. The next tap throws a bomb instead of a ball and spends no
        /// ball, since the bomb is the booster the player paid for.
        /// </summary>
        public void ArmBomb()
        {
            bombArmed = true;
        }

        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;

            // The opening Home is never broadcast (ChangeState skips a no-op move), so read the
            // live state here to start hidden on the menu.
            ApplyVisibility(GameManager.Instance != null
                ? GameManager.Instance.CurrentState
                : GameState.Home);
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState state)
        {
            ApplyVisibility(state);
        }

        private void ApplyVisibility(GameState state)
        {
            GameObject target = visualRoot != null
                ? visualRoot
                : (aimPivot != null ? aimPivot.gameObject : null);

            if (target != null)
            {
                target.SetActive(state == GameState.Playing);
            }
        }

        private void Update()
        {
            // Only the live level takes shots; taps on Home or an end panel do nothing.
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying)
            {
                return;
            }

            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                return;
            }

            if (!pointer.press.wasPressedThisFrame)
            {
                return;
            }

            if (IsPointerOverUI())
            {
                return;
            }

            Fire(pointer.position.ReadValue());
        }


        private bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        private void Fire(Vector2 screenPosition)
        {
            if (bombArmed)
            {
                FireBomb(screenPosition);
                bombArmed = false;
                BombFired?.Invoke();
                return;
            }

            if (!LevelManager.Instance.TryConsumeBall())
            {
                return;
            }

            Vector3 aimPoint = GetAimPoint(screenPosition);
            AimAt(aimPoint);
            Vector3 direction = (aimPoint - muzzle.position).normalized;

            GameObject instance = PoolManager.Instance.Spawn(
                ballPoolId,
                muzzle.position,
                Quaternion.LookRotation(direction));

            if (instance.TryGetComponent(out Ball ball))
            {
                ball.Launch(direction * ballSpeed);
            }

            FlashMuzzle(direction);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShoot();
            }
        }

        private void FireBomb(Vector2 screenPosition)
        {
            Vector3 aimPoint = GetAimPoint(screenPosition);
            AimAt(aimPoint);
            Vector3 direction = (aimPoint - muzzle.position).normalized;

            GameObject instance = PoolManager.Instance.Spawn(
                bombPoolId,
                muzzle.position,
                Quaternion.LookRotation(direction));

            if (instance != null && instance.TryGetComponent(out Bomb bomb))
            {
                bomb.Launch(direction * bombSpeed, aimPoint);
            }

            FlashMuzzle(direction);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayShoot();
            }
        }

        /// <summary>
        /// Swings the barrel so its length points at where the player tapped. The barrel mesh
        /// runs up its local +Y, so we rotate that axis onto the aim direction; a small floor on
        /// the pitch keeps it from ever dipping below the horizon and looking broken.
        /// </summary>
        private void AimAt(Vector3 aimPoint)
        {
            if (aimPivot == null)
            {
                return;
            }

            Vector3 direction = (aimPoint - aimPivot.position).normalized;

            if (direction.y < 0.05f)
            {
                direction.y = 0.05f;
                direction = direction.normalized;
            }

            aimPivot.rotation = Quaternion.FromToRotation(Vector3.up, direction);
        }

        private void FlashMuzzle(Vector3 direction)
        {
            if (string.IsNullOrEmpty(muzzleFlashId) || PoolManager.Instance == null)
            {
                return;
            }

            PoolManager.Instance.Spawn(
                muzzleFlashId,
                muzzle.position,
                Quaternion.LookRotation(direction));
        }

        private Vector3 GetAimPoint(Vector2 screenPosition)
        {
            Ray ray = gameCamera.ScreenPointToRay(screenPosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimLayers))
            {
                return hit.point;
            }

            return ray.GetPoint(maxAimDistance);
        }
    }
}
