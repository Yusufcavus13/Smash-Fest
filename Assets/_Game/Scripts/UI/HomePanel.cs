using SmashFest.Core;
using SmashFest.Levels;
using SmashFest.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{

    public class HomePanel : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text playButtonText;
        [SerializeField] private Button playButton;
        [SerializeField] private Image playButtonImage;

        [Header("Difficulty")]
        [SerializeField] private DifficultyPalette palette;
        [Tooltip("Warning icons and the label; hidden on a normal level.")]
        [SerializeField] private GameObject playDifficultyRow;
        [SerializeField] private TMP_Text playDifficultyText;

        [Header("Out Of Lives")]
        [Tooltip("Opened instead of starting a level when the player has no lives left.")]
        [SerializeField] private OutOfLivesPanel outOfLivesPanel;

        [Header("Texts")]
        [SerializeField] private string comingSoonText = "Coming Soon";

        // --- Unity Messages ---

        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;

            playButton.onClick.AddListener(HandlePlayClicked);
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;

            playButton.onClick.RemoveListener(HandlePlayClicked);
        }

        private void Start()
        {
            HandleStateChanged(GameManager.Instance.CurrentState);
        }



        private void HandleStateChanged(GameState state)
        {
            if (state != GameState.Home)
            {
                panelRoot.SetActive(false);
                return;
            }

            Refresh();
            panelRoot.SetActive(true);
        }

        private void Refresh()
        {
            LevelManager levels = LevelManager.Instance;

            if (levels.HasNextLevel)
            {
                int levelNumber = levels.NextLevelNumber;
                LevelType levelType = LevelRules.GetLevelType(levelNumber);
                DifficultyPalette.Skin skin = palette.Get(levelType);

                playButtonText.text = $"Level {levelNumber}";
                playButtonImage.sprite = skin.wideButton;
                playDifficultyText.text = skin.label;
                playDifficultyRow.SetActive(levelType != LevelType.Normal);

                playButton.interactable = true;
            }
            else
            {
                playButtonText.text = comingSoonText;
                playDifficultyRow.SetActive(false);
                playButton.interactable = false;
            }
        }

        private void HandlePlayClicked()
        {
            if (!EconomyManager.Instance.HasLives)
            {
                outOfLivesPanel.Show();
                return;
            }

            LevelManager.Instance.StartNextLevel();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PanelRootValidator.Validate(this, panelRoot, nameof(panelRoot));
        }
#endif
    }
}
