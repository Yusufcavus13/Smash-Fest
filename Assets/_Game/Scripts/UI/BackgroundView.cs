using SmashFest.Core;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{

    public class BackgroundView : MonoBehaviour
    {


        [Header("References")]
        [SerializeField] private Image backgroundImage;

        [Header("Sprites")]
        [SerializeField] private Sprite menuBackground;
        [SerializeField] private Sprite levelBackground;


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
            HandleStateChanged(GameManager.Instance.CurrentState);
        }

        private void HandleStateChanged(GameState state)
        {
            backgroundImage.sprite = state == GameState.Home ? menuBackground : levelBackground;
        }
    }
}
