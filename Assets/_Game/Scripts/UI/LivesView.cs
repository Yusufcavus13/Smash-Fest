using System;
using SmashFest.Systems;
using TMPro;
using UnityEngine;

namespace SmashFest.UI
{
    /// <summary>
    /// Lives badge with the countdown to the next life. Drop it on any lives badge.
    /// </summary>
    public class LivesView : MonoBehaviour
    {
        // --- Serialized Fields ---

        [Header("References")]
        [SerializeField] private TMP_Text livesText;
        [SerializeField] private TMP_Text timerText;

        [Header("Texts")]
        [SerializeField] private string fullText = "FULL";

        // --- Fields ---

        private bool isReady;

        // --- Unity Messages ---

        private void OnEnable()
        {
            EconomyManager.LivesChanged += HandleLivesChanged;

            if (isReady)
            {
                HandleLivesChanged(EconomyManager.Instance.Lives);
            }
        }

        private void OnDisable()
        {
            EconomyManager.LivesChanged -= HandleLivesChanged;
        }

        private void Start()
        {
            isReady = true;
            HandleLivesChanged(EconomyManager.Instance.Lives);
        }

        private void Update()
        {
            if (!isReady)
            {
                return;
            }

            RefreshTimer();
        }

        // --- Private Methods ---

        private void HandleLivesChanged(int lives)
        {
            livesText.text = lives.ToString();
            RefreshTimer();
        }

        private void RefreshTimer()
        {
            EconomyManager economy = EconomyManager.Instance;

            if (economy.IsLivesFull)
            {
                timerText.text = fullText;
                return;
            }

            TimeSpan remaining = economy.TimeUntilNextLife;
            timerText.text = $"{remaining.Minutes + remaining.Hours * 60:00}:{remaining.Seconds:00}";
        }
    }
}
