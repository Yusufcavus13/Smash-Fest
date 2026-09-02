using System.Collections;
using SmashFest.Core;
using SmashFest.Levels;
using TMPro;
using UnityEngine;

namespace SmashFest.UI
{
    /// <summary>
    /// Winning has no panel and no button. The logo assembles itself piece by piece over
    /// the confetti, the reward lands last, and the game returns to the menu on its own.
    /// </summary>
    public class WinCelebration : MonoBehaviour
    {
        // --- Serialized Fields ---

        [Header("References")]
        [SerializeField] private GameObject bannerRoot;
        [SerializeField] private ParticleSystem confetti;
        [SerializeField] private TMP_Text rewardText;

        [Header("Logo Parts")]
        [Tooltip("Animated in this order, one after the other.")]
        [SerializeField] private RectTransform raysTransform;
        [SerializeField] private RectTransform frameTransform;
        [SerializeField] private RectTransform smashTransform;
        [SerializeField] private RectTransform festTransform;
        [SerializeField] private RectTransform rewardTransform;

        [Header("Animation")]
        [SerializeField] private float popDuration = 0.22f;
        [SerializeField] private float popOvershoot = 1.18f;

        [Tooltip("Pause between two parts appearing.")]
        [SerializeField] private float partGap = 0.05f;

        [SerializeField] private float raysSpinSpeed = 9f;

        [Tooltip("Seconds the finished logo is held before returning to the menu.")]
        [SerializeField] private float holdDuration = 1.6f;

        // --- Fields ---

        private Coroutine celebrateRoutine;
        private Coroutine spinRoutine;

        // --- Unity Messages ---

        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;
        }

        private void Start()
        {
            bannerRoot.SetActive(false);
        }

        // --- Private Methods ---

        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.LevelComplete)
            {
                celebrateRoutine = StartCoroutine(CelebrateRoutine());
                return;
            }

            StopCelebration();
            bannerRoot.SetActive(false);
        }

        private IEnumerator CelebrateRoutine()
        {
            int levelNumber = LevelManager.Instance.CurrentLevelNumber;
            rewardText.text = $"+{LevelRules.GetReward(LevelRules.GetLevelType(levelNumber))}";

            HideParts();
            bannerRoot.SetActive(true);
            confetti.Play();

            spinRoutine = StartCoroutine(SpinRaysRoutine());

            yield return PopRoutine(raysTransform);
            yield return new WaitForSeconds(partGap);
            yield return PopRoutine(frameTransform);
            yield return new WaitForSeconds(partGap);
            yield return PopRoutine(smashTransform);
            yield return new WaitForSeconds(partGap);
            yield return PopRoutine(festTransform);
            yield return new WaitForSeconds(partGap);
            yield return PopRoutine(rewardTransform);

            yield return new WaitForSeconds(holdDuration);

            StopSpin();
            celebrateRoutine = null;
            bannerRoot.SetActive(false);

            LevelManager.Instance.GoHome();
        }

        private void HideParts()
        {
            raysTransform.localScale = Vector3.zero;
            frameTransform.localScale = Vector3.zero;
            smashTransform.localScale = Vector3.zero;
            festTransform.localScale = Vector3.zero;
            rewardTransform.localScale = Vector3.zero;

            raysTransform.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// Scales a part from nothing, past its final size, then settles. The overshoot is
        /// what makes it feel like the piece lands instead of just appearing.
        /// </summary>
        private IEnumerator PopRoutine(RectTransform target)
        {
            float elapsed = 0f;

            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsed / popDuration);
                float scale = progress < 0.65f
                    ? Mathf.Lerp(0f, popOvershoot, progress / 0.65f)
                    : Mathf.Lerp(popOvershoot, 1f, (progress - 0.65f) / 0.35f);

                target.localScale = Vector3.one * scale;

                yield return null;
            }

            target.localScale = Vector3.one;
        }

        private IEnumerator SpinRaysRoutine()
        {
            while (true)
            {
                raysTransform.Rotate(0f, 0f, raysSpinSpeed * Time.deltaTime);
                yield return null;
            }
        }

        private void StopSpin()
        {
            if (spinRoutine == null)
            {
                return;
            }

            StopCoroutine(spinRoutine);
            spinRoutine = null;
        }

        /// <summary>
        /// Used when the celebration is cut short by a state change. Never call this from
        /// inside the celebration coroutine, it would stop the caller mid way.
        /// </summary>
        private void StopCelebration()
        {
            StopSpin();

            if (celebrateRoutine == null)
            {
                return;
            }

            StopCoroutine(celebrateRoutine);
            celebrateRoutine = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PanelRootValidator.Validate(this, bannerRoot, nameof(bannerRoot));
        }
#endif
    }
}
