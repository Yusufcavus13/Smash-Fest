using System;
using SmashFest.Core;
using UnityEngine;

namespace SmashFest.Systems
{
    /// <summary>
    /// Owns the player wallet: coins and lives. Every purchase goes through a Try method,
    /// so no caller can spend what the player does not have. Lives refill by themselves
    /// over real time, even while the game is closed.
    /// </summary>
    public class EconomyManager : MonoSingleton<EconomyManager>
    {
        // --- Events ---

        public static event Action<int> CoinsChanged;
        public static event Action<int> LivesChanged;

        // --- Properties ---

        public int Coins { get; private set; }
        public int Lives { get; private set; }

        public bool HasLives => Lives > 0;
        public bool IsLivesFull => Lives >= maxLives;

        public int ContinueCost => continueCost;
        public int ContinueBallCount => continueBallCount;
        public int RefillLivesCost => refillLivesCost;
        public int MaxLives => maxLives;

        /// <summary>
        /// How long until one more life arrives. Zero when the lives are already full.
        /// </summary>
        public TimeSpan TimeUntilNextLife
        {
            get
            {
                if (IsLivesFull || nextLifeTicks == 0L)
                {
                    return TimeSpan.Zero;
                }

                long remaining = nextLifeTicks - DateTime.UtcNow.Ticks;
                return remaining <= 0L ? TimeSpan.Zero : new TimeSpan(remaining);
            }
        }

        // --- Serialized Fields ---

        [Header("Starting Values")]
        [SerializeField] private int startingCoins = 1000;
        [SerializeField] private int maxLives = 5;

        [Header("Prices")]
        [Tooltip("Coins asked for the extra balls on the continue screen.")]
        [SerializeField] private int continueCost = 900;

        [SerializeField] private int continueBallCount = 5;

        [Tooltip("Coins asked to refill the lives.")]
        [SerializeField] private int refillLivesCost = 900;

        [Header("Life Regeneration")]
        [Tooltip("Real seconds needed to earn one life back.")]
        [SerializeField] private int lifeRegenSeconds = 1800;

        // --- Fields ---

        private long nextLifeTicks;
        private float regenCheckTimer;

        // --- Unity Messages ---

        private void Start()
        {
            CoinsChanged?.Invoke(Coins);
            LivesChanged?.Invoke(Lives);
        }

        private void Update()
        {
            if (IsLivesFull)
            {
                return;
            }

            regenCheckTimer += Time.unscaledDeltaTime;
            if (regenCheckTimer < 1f)
            {
                return;
            }

            regenCheckTimer = 0f;
            ApplyPendingRegeneration();
        }

        // --- Public Methods ---

        public void AddCoins(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            Coins += amount;
            SaveManager.SaveCoins(Coins);
            CoinsChanged?.Invoke(Coins);
        }

        public bool CanAfford(int amount)
        {
            return Coins >= amount;
        }

        public bool TrySpendCoins(int amount)
        {
            if (!CanAfford(amount))
            {
                return false;
            }

            Coins -= amount;
            SaveManager.SaveCoins(Coins);
            CoinsChanged?.Invoke(Coins);

            return true;
        }

        public bool TryConsumeLife()
        {
            if (Lives <= 0)
            {
                return false;
            }

            bool wasFull = IsLivesFull;

            Lives--;
            SaveManager.SaveLives(Lives);

            if (wasFull)
            {
                StartRegenTimer();
            }

            LivesChanged?.Invoke(Lives);

            return true;
        }

        public bool TryRefillLives()
        {
            if (IsLivesFull)
            {
                return false;
            }

            if (!TrySpendCoins(refillLivesCost))
            {
                return false;
            }

            Lives = maxLives;
            nextLifeTicks = 0L;

            SaveManager.SaveLives(Lives);
            SaveManager.SaveNextLifeTicks(nextLifeTicks);
            LivesChanged?.Invoke(Lives);

            return true;
        }

        [ContextMenu("Reset Progress")]
        public void ResetProgress()
        {
            SaveManager.ResetProgress();

            Coins = startingCoins;
            Lives = maxLives;
            nextLifeTicks = 0L;

            CoinsChanged?.Invoke(Coins);
            LivesChanged?.Invoke(Lives);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only escape hatch behind the dev window. Testing the retry screen means
        /// running the lives down, and without this the only way back is to wait out the
        /// regen timer.
        /// </summary>
        public void EditorApplyWallet(int coins, int lives)
        {
            Coins = Mathf.Max(0, coins);
            Lives = Mathf.Clamp(lives, 0, maxLives);
            nextLifeTicks = IsLivesFull
                ? 0L
                : DateTime.UtcNow.Ticks + TimeSpan.TicksPerSecond * lifeRegenSeconds;

            SaveManager.SaveCoins(Coins);
            SaveManager.SaveLives(Lives);
            SaveManager.SaveNextLifeTicks(nextLifeTicks);

            CoinsChanged?.Invoke(Coins);
            LivesChanged?.Invoke(Lives);
        }
#endif

        // --- Protected Methods ---

        protected override void OnSingletonAwake()
        {
            Coins = SaveManager.LoadCoins(startingCoins);
            Lives = Mathf.Clamp(SaveManager.LoadLives(maxLives), 0, maxLives);
            nextLifeTicks = SaveManager.LoadNextLifeTicks();

            ApplyPendingRegeneration();
        }

        // --- Private Methods ---

        /// <summary>
        /// Grants every life the player earned since the timer was last checked, including
        /// the time the game spent closed.
        /// </summary>
        private void ApplyPendingRegeneration()
        {
            if (IsLivesFull)
            {
                return;
            }

            if (nextLifeTicks == 0L)
            {
                StartRegenTimer();
                return;
            }

            long intervalTicks = TimeSpan.TicksPerSecond * lifeRegenSeconds;
            long now = DateTime.UtcNow.Ticks;
            bool granted = false;

            while (Lives < maxLives && now >= nextLifeTicks)
            {
                Lives++;
                nextLifeTicks += intervalTicks;
                granted = true;
            }

            if (!granted)
            {
                return;
            }

            if (IsLivesFull)
            {
                nextLifeTicks = 0L;
            }

            SaveManager.SaveLives(Lives);
            SaveManager.SaveNextLifeTicks(nextLifeTicks);
            LivesChanged?.Invoke(Lives);
        }

        private void StartRegenTimer()
        {
            nextLifeTicks = DateTime.UtcNow.Ticks + TimeSpan.TicksPerSecond * lifeRegenSeconds;
            SaveManager.SaveNextLifeTicks(nextLifeTicks);
        }
    }
}
