using SmashFest.Core;
using SmashFest.Gameplay.Shooting;
using SmashFest.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{
    /// <summary>
    /// The in-game bomb booster. Tapping it once buys a bomb with coins and arms the cannon;
    /// the player's next tap throws it. It only shows while a level is actually being played.
    /// </summary>
    public class BoosterButton : MonoBehaviour
    {
        // --- Serialized Fields ---

        [Header("References")]
        [SerializeField] private CannonController cannon;
        [SerializeField] private GameObject root;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text priceText;

        [Tooltip("Coin icon and price, hidden once the bomb is armed.")]
        [SerializeField] private GameObject priceGroup;

        [Tooltip("Shown while the bomb is armed and waiting to be thrown.")]
        [SerializeField] private GameObject armedBadge;

        [Header("Cost")]
        [SerializeField] private int price = 900;

        // --- Unity Messages ---

        private void OnEnable()
        {
            button.onClick.AddListener(HandleClicked);
            GameManager.StateChanged += HandleStateChanged;
            EconomyManager.CoinsChanged += HandleCoinsChanged;
            CannonController.BombFired += HandleBombFired;

            // HUD is toggled by HudView, so re-read the state every time we come back on.
            if (GameManager.Instance != null)
            {
                priceText.text = price.ToString();
                HandleStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnDisable()
        {
            button.onClick.RemoveListener(HandleClicked);
            GameManager.StateChanged -= HandleStateChanged;
            EconomyManager.CoinsChanged -= HandleCoinsChanged;
            CannonController.BombFired -= HandleBombFired;
        }

        // --- Private Methods ---

        private void HandleClicked()
        {
            // Already armed: the tap belongs to the throw, not another purchase.
            if (cannon.BombArmed)
            {
                return;
            }

            if (!EconomyManager.Instance.TrySpendCoins(price))
            {
                return;
            }

            cannon.ArmBomb();
            SetArmed(true);
        }

        private void HandleBombFired()
        {
            SetArmed(false);
        }

        private void SetArmed(bool armed)
        {
            if (priceGroup != null)
            {
                priceGroup.SetActive(!armed);
            }

            if (armedBadge != null)
            {
                armedBadge.SetActive(armed);
            }

            RefreshAffordable();
        }

        private void HandleStateChanged(GameState state)
        {
            bool inPlay = state == GameState.Playing || state == GameState.OutOfBalls;
            root.SetActive(inPlay);

            if (inPlay)
            {
                SetArmed(cannon.BombArmed);
            }
        }

        private void HandleCoinsChanged(int coins)
        {
            RefreshAffordable();
        }

        private void RefreshAffordable()
        {
            // Once armed the button stays lit; otherwise it dims when the player is short.
            button.interactable = cannon.BombArmed || EconomyManager.Instance.CanAfford(price);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PanelRootValidator.Validate(this, root, nameof(root));
        }
#endif
    }
}
