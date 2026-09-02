using System;
using UnityEngine;

namespace SmashFest.Core
{
    public class GameManager : MonoSingleton<GameManager>
    {

        public static event Action<GameState> StateChanged;

        public GameState CurrentState { get; private set; } = GameState.Home;
        public bool IsPlaying => CurrentState == GameState.Playing;

        [Header("Debug")]
        [SerializeField] private bool logStateChanges = true;


        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState)
            {
                return;
            }

            CurrentState = newState;

            if (logStateChanges)
            {
                Debug.Log($"[GameManager] State -> {newState}");
            }

            StateChanged?.Invoke(newState);
        }
    }
}
