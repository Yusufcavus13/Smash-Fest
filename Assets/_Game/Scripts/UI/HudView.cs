using System.Collections;
using SmashFest.Core;
using SmashFest.Levels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{
  
    public class HudView : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private GameObject hudRoot;
        [SerializeField] private TMP_Text ballsText;
        [SerializeField] private RectTransform ballsBadge;

        [Header("Difficulty")]
        [SerializeField] private DifficultyPalette palette;
        [SerializeField] private Image ballsBadgeImage;
        [SerializeField] private Image settingsButtonImage;

        [Header("Feedback")]
        [SerializeField] private float punchScale = 1.2f;
        [SerializeField] private float punchDuration = 0.14f;

        private Coroutine punchRoutine;

        private void OnEnable()
        {
            LevelManager.BallsChanged += HandleBallsChanged;
            LevelManager.LevelStarted += HandleLevelStarted;
            GameManager.StateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            LevelManager.BallsChanged -= HandleBallsChanged;
            LevelManager.LevelStarted -= HandleLevelStarted;
            GameManager.StateChanged -= HandleStateChanged;
        }
        private void Start()
        {
            HandleStateChanged(GameManager.Instance.CurrentState);
        }

        private void HandleStateChanged(GameState state)
        {
            hudRoot.SetActive(state == GameState.Playing || state == GameState.OutOfBalls);
        }
        private void HandleLevelStarted(int levelNumber)
        {
            DifficultyPalette.Skin skin = palette.GetForLevel(levelNumber);

            ballsBadgeImage.sprite = skin.badge;
            settingsButtonImage.sprite = skin.settingsButton;
        }

        private void HandleBallsChanged(int ballsRemaining)
        {
            ballsText.text = ballsRemaining.ToString();

            if (punchRoutine != null)
            {
                StopCoroutine(punchRoutine);
            }

            punchRoutine = StartCoroutine(PunchRoutine());
        }

        private IEnumerator PunchRoutine()
        {
            float elapsed = 0f;

            while (elapsed < punchDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                float progress = elapsed / punchDuration;
                float scale = Mathf.Lerp(punchScale, 1f, progress);
                ballsBadge.localScale = Vector3.one * scale;

                yield return null;
            }

            ballsBadge.localScale = Vector3.one;
            punchRoutine = null;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PanelRootValidator.Validate(this, hudRoot, nameof(hudRoot));
        }
#endif
    }
}
