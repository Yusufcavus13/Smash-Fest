using SmashFest.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{

    public class OutOfLivesPanel : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private TMP_Text refillPriceText;
        [SerializeField] private Button refillButton;
        [SerializeField] private Button closeButton;

        [Header("Texts")]
        [SerializeField] private string readyText = "A life is ready!";


        private void OnEnable()
        {
            refillButton.onClick.AddListener(HandleRefillClicked);
            closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            refillButton.onClick.RemoveListener(HandleRefillClicked);
            closeButton.onClick.RemoveListener(Close);
        }

        private void Start()
        {
            panelRoot.SetActive(false);
        }

        private void Update()
        {
            if (panelRoot.activeSelf)
            {
                RefreshTimer();
            }
        }


        public void Show()
        {
            Refresh();
            panelRoot.SetActive(true);
        }

        public void Close()
        {
            panelRoot.SetActive(false);
        }

        // --- Private Methods ---

        private void Refresh()
        {
            EconomyManager economy = EconomyManager.Instance;

            livesText.text = economy.Lives.ToString();
            refillPriceText.text = economy.RefillLivesCost.ToString();
            refillButton.interactable = economy.CanAfford(economy.RefillLivesCost);

            RefreshTimer();
        }

        private void RefreshTimer()
        {
            System.TimeSpan remaining = EconomyManager.Instance.TimeUntilNextLife;

            timerText.text = remaining <= System.TimeSpan.Zero
                ? readyText
                : $"Next life in {remaining:mm\\:ss}";
        }

        private void HandleRefillClicked()
        {
            if (EconomyManager.Instance.TryRefillLives())
            {
                Close();
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
