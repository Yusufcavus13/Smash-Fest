using System.Collections;
using SmashFest.Core;
using SmashFest.Levels;
using SmashFest.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{
    public class ContinuePanel : MonoBehaviour
    {


        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private RectTransform windowTransform;
        [SerializeField] private TMP_Text amountText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text priceText;
        [SerializeField] private Button playOnButton;
        [SerializeField] private Button closeButton;

        [Header("Denied Feedback")]
        [SerializeField] private float shakeDuration = 0.3f;
        [SerializeField] private float shakeStrength = 26f;



        private Coroutine shakeRoutine;


        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;
            playOnButton.onClick.AddListener(HandlePlayOnClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;
            playOnButton.onClick.RemoveListener(HandlePlayOnClicked);
            closeButton.onClick.RemoveListener(HandleCloseClicked);
        }

        private void Start()
        {
            panelRoot.SetActive(false);
        }


        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.OutOfBalls)
            {
                Show();
                return;
            }

            panelRoot.SetActive(false);
        }

        private void Show()
        {
            EconomyManager economy = EconomyManager.Instance;

            amountText.text = $"+{economy.ContinueBallCount}";
            descriptionText.text = $"Add {economy.ContinueBallCount} balls to continue!";
            priceText.text = economy.ContinueCost.ToString();

            panelRoot.SetActive(true);
        }

        private void HandlePlayOnClicked()
        {
            if (LevelManager.Instance.TryContinueWithExtraBalls())
            {
                return;
            }

            PlayDeniedFeedback();
        }

        private void HandleCloseClicked()
        {
            LevelManager.Instance.GiveUp();
        }

        private void PlayDeniedFeedback()
        {
            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            Vector2 origin = windowTransform.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float damping = 1f - elapsed / shakeDuration;
                float offset = Mathf.Sin(elapsed * 60f) * shakeStrength * damping;
                windowTransform.anchoredPosition = origin + new Vector2(offset, 0f);

                yield return null;
            }

            windowTransform.anchoredPosition = origin;
            shakeRoutine = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PanelRootValidator.Validate(this, panelRoot, nameof(panelRoot));
        }
#endif
    }
}
