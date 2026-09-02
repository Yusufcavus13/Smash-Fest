using SmashFest.Core;
using SmashFest.Gameplay.Shooting;
using SmashFest.Levels;
using SmashFest.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{
    public class RetryPanel : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text difficultyText;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private Button tryAgainButton;
        [SerializeField] private Button refillLivesButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text refillPriceText;

        [Header("Difficulty Skin")]
        [SerializeField] private Image panelImage;
        [SerializeField] private Image bannerImage;
        [Tooltip("Shown only when the level is not a normal one.")]
        [SerializeField] private GameObject difficultyRow;

        [Header("Card")]
        [Tooltip("Booster picker, shown while the player still has a life to spend.")]
        [SerializeField] private GameObject boosterRow;
        [Tooltip("Remaining lives, shown once they have run out and a refill is the only way on.")]
        [SerializeField] private GameObject livesRow;
        [SerializeField] private DifficultyPalette palette;

        [Header("Booster")]
        [SerializeField] private CannonController cannon;
        [Tooltip("Tapping the booster slot buys a bomb for the retry.")]
        [SerializeField] private Button boosterButton;
        [Tooltip("Shown once the bomb booster is bought for this retry.")]
        [SerializeField] private GameObject boosterSelectedBadge;
        [SerializeField] private int boosterPrice = 900;

        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;
            tryAgainButton.onClick.AddListener(HandleTryAgainClicked);
            refillLivesButton.onClick.AddListener(HandleRefillClicked);
            closeButton.onClick.AddListener(HandleCloseClicked);

            if (boosterButton != null)
            {
                boosterButton.onClick.AddListener(HandleBoosterClicked);
            }
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;
            tryAgainButton.onClick.RemoveListener(HandleTryAgainClicked);
            refillLivesButton.onClick.RemoveListener(HandleRefillClicked);
            closeButton.onClick.RemoveListener(HandleCloseClicked);

            if (boosterButton != null)
            {
                boosterButton.onClick.RemoveListener(HandleBoosterClicked);
            }
        }

        private void Start()
        {
            panelRoot.SetActive(false);
        }
        private void HandleStateChanged(GameState state)
        {
            if (state == GameState.LevelFailed)
            {
                Show();
                return;
            }

            panelRoot.SetActive(false);
        }

        private void Show()
        {
            int levelNumber = LevelManager.Instance.CurrentLevelNumber;
            LevelType levelType = LevelRules.GetLevelType(levelNumber);

            DifficultyPalette.Skin skin = palette.Get(levelType);

            levelText.text = $"Level {levelNumber}";
            difficultyText.text = skin.label;
            difficultyRow.SetActive(levelType != LevelType.Normal);

            panelImage.sprite = skin.panel;
            bannerImage.sprite = skin.banner;

            RefreshLives();
            RefreshBooster();
            panelRoot.SetActive(true);
        }

        private void RefreshBooster()
        {
            // The bomb may already be armed from a previous pick; reflect that here.
            bool armed = cannon != null && cannon.BombArmed;

            if (boosterSelectedBadge != null)
            {
                boosterSelectedBadge.SetActive(armed);
            }

            if (boosterButton != null)
            {
                boosterButton.interactable = armed || EconomyManager.Instance.CanAfford(boosterPrice);
            }
        }

        private void HandleBoosterClicked()
        {
            if (cannon == null || cannon.BombArmed)
            {
                return;
            }

            if (!EconomyManager.Instance.TrySpendCoins(boosterPrice))
            {
                return;
            }

            cannon.ArmBomb();
            RefreshBooster();
        }

        private void RefreshLives()
        {
            EconomyManager economy = EconomyManager.Instance;
            bool hasLives = economy.Lives > 0;

            livesText.text = economy.Lives.ToString();
            refillPriceText.text = economy.RefillLivesCost.ToString();

            tryAgainButton.gameObject.SetActive(hasLives);
            refillLivesButton.gameObject.SetActive(!hasLives);

            boosterRow.SetActive(hasLives);
            livesRow.SetActive(!hasLives);
        }

        private void HandleTryAgainClicked()
        {
            LevelManager.Instance.ReloadLevel();
        }

        private void HandleCloseClicked()
        {
            LevelManager.Instance.GoHome();
        }

        private void HandleRefillClicked()
        {
            if (EconomyManager.Instance.TryRefillLives())
            {
                RefreshLives();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PanelRootValidator.Validate(this, panelRoot, nameof(panelRoot));
        }
#endif
    }
}
