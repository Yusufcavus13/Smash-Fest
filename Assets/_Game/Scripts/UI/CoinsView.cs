using SmashFest.Systems;
using TMPro;
using UnityEngine;

namespace SmashFest.UI
{

    public class CoinsView : MonoBehaviour
    {

        [Header("References")]
        [SerializeField] private TMP_Text coinsText;

        private bool isReady;

        private void OnEnable()
        {
            EconomyManager.CoinsChanged += HandleCoinsChanged;

            if (isReady)
            {
                HandleCoinsChanged(EconomyManager.Instance.Coins);
            }
        }

        private void OnDisable()
        {
            EconomyManager.CoinsChanged -= HandleCoinsChanged;
        }
        private void Start()
        {
            isReady = true;
            HandleCoinsChanged(EconomyManager.Instance.Coins);
        }
        private void HandleCoinsChanged(int coins)
        {
            coinsText.text = coins.ToString();
        }
    }
}
