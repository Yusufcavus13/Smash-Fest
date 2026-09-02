using System;
using System.Collections;
using System.Collections.Generic;
using SmashFest.Core;
using SmashFest.Gameplay.Objects;
using SmashFest.Systems;
using UnityEngine;

namespace SmashFest.Levels
{

    public class LevelManager : MonoSingleton<LevelManager>
    {


        public static event Action<int> LevelStarted;
        public static event Action<int> BallsChanged;
        public static event Action<int> ObjectsRemainingChanged;


        public int CurrentLevelNumber { get; private set; }
        public int NextLevelNumber { get; private set; }
        public bool HasNextLevel { get; private set; }
        public int BallsRemaining { get; private set; }
        public int ObjectsRemaining { get; private set; }

  

        [Header("References")]
        [Tooltip("Origin of the level layout. Json positions are local to this transform. "
            + "Replaced at runtime by the platform the level asks for.")]
        [SerializeField] private Transform objectsRoot;

        [Header("Platforms")]
        [SerializeField] private PlatformLibrary platformLibrary;

        [Tooltip("Spawned platforms are parented here.")]
        [SerializeField] private Transform platformParent;

        [Header("Level")]
        [Tooltip("Used only when there is no saved progress yet.")]
        [SerializeField] private int firstLevelNumber = 1;

        [Tooltip("Seconds to wait for the physics to settle before offering a continue.")]
        [SerializeField] private float settleDelay = 2.5f;

        [Header("Clearing")]
        [Tooltip("An object counts as cleared once it drops this far below the platform surface.")]
        [SerializeField] private float clearDropDistance = 0.3f;

        [Tooltip("How often the height of the remaining objects is checked, in seconds.")]
        [SerializeField] private float clearCheckInterval = 0.1f;



        private readonly List<LevelObject> spawnedObjects = new List<LevelObject>();

        private WaitForSeconds settleWait;
        private WaitForSeconds clearCheckWait;
        private Coroutine failCheckRoutine;
        private PlatformPreset activePlatform;
        private string activePlatformId;



        private void OnEnable()
        {
            LevelObject.Cleared += HandleObjectCleared;
        }

        private void OnDisable()
        {
            LevelObject.Cleared -= HandleObjectCleared;
        }

        private void Start()
        {
            StartCoroutine(ClearCheckRoutine());

            GoHome();
        }




        [ContextMenu("Start Next Level")]
        public void StartNextLevel()
        {
            LoadLevel(NextLevelNumber);
        }

        [ContextMenu("Go Home")]
        public void GoHome()
        {
            StopFailCheck();
            DespawnLevel();

            NextLevelNumber = SaveManager.LoadLevel(firstLevelNumber);
            HasNextLevel = LevelLoader.Exists(NextLevelNumber);

            GameManager.Instance.ChangeState(GameState.Home);
        }

        public void LoadLevel(int levelNumber)
        {
            LevelData data = LevelLoader.Load(levelNumber);
            if (data == null)
            {
                return;
            }

            StopFailCheck();
            DespawnLevel();

            ApplyPlatform(data.platformId);

            CurrentLevelNumber = levelNumber;
            BallsRemaining = data.ballCount;
            ObjectsRemaining = 0;

            for (int i = 0; i < data.objects.Length; i++)
            {
                SpawnLevelObject(data.objects[i]);
            }

            LevelStarted?.Invoke(levelNumber);
            BallsChanged?.Invoke(BallsRemaining);
            ObjectsRemainingChanged?.Invoke(ObjectsRemaining);

            Debug.Log($"[LevelManager] Level {levelNumber} ({LevelRules.GetLevelType(levelNumber)}) " +
                $"started with {ObjectsRemaining} objects and {BallsRemaining} balls.");

            GameManager.Instance.ChangeState(GameState.Playing);
        }

        [ContextMenu("Reload Level")]
        public void ReloadLevel()
        {
            LoadLevel(CurrentLevelNumber);
        }

        public bool TryContinueWithExtraBalls()
        {
            EconomyManager economy = EconomyManager.Instance;

            if (!economy.TrySpendCoins(economy.ContinueCost))
            {
                return false;
            }

            BallsRemaining += economy.ContinueBallCount;
            BallsChanged?.Invoke(BallsRemaining);

            GameManager.Instance.ChangeState(GameState.Playing);

            return true;
        }

        public void GiveUp()
        {
            EconomyManager.Instance.TryConsumeLife();
            GameManager.Instance.ChangeState(GameState.LevelFailed);
        }

        public bool TryConsumeBall()
        {
            if (!GameManager.Instance.IsPlaying)
            {
                return false;
            }

            if (BallsRemaining <= 0)
            {
                return false;
            }

            BallsRemaining--;
            BallsChanged?.Invoke(BallsRemaining);

            if (BallsRemaining == 0)
            {
                failCheckRoutine = StartCoroutine(FailCheckRoutine());
            }

            return true;
        }

        protected override void OnSingletonAwake()
        {
            settleWait = new WaitForSeconds(settleDelay);
            clearCheckWait = new WaitForSeconds(clearCheckInterval);
            NextLevelNumber = SaveManager.LoadLevel(firstLevelNumber);
            HasNextLevel = LevelLoader.Exists(NextLevelNumber);
        }



        /// <summary>
        /// Swaps in the layout this level asks for. Rebuilding only on a change keeps the
        /// common case, level after level on the same platform, free.
        /// </summary>
        private void ApplyPlatform(string platformId)
        {
            if (platformLibrary == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(platformId))
            {
                platformId = platformLibrary.FallbackId;
            }

            if (platformId == activePlatformId && activePlatform != null)
            {
                return;
            }

            PlatformPreset prefab = platformLibrary.Get(platformId);
            if (prefab == null)
            {
                return;
            }

            if (activePlatform != null)
            {
                Destroy(activePlatform.gameObject);
            }

            activePlatform = Instantiate(prefab, platformParent);
            activePlatformId = platformId;
            objectsRoot = activePlatform.ObjectsRoot;
        }

        private void SpawnLevelObject(LevelObjectData objectData)
        {
            Vector3 worldPosition = objectsRoot.TransformPoint(objectData.position);
            Quaternion worldRotation = objectsRoot.rotation * Quaternion.Euler(objectData.rotation);

            GameObject instance = PoolManager.Instance.Spawn(objectData.id, worldPosition, worldRotation);
            if (instance == null)
            {
                return;
            }

            if (!instance.TryGetComponent(out LevelObject levelObject))
            {
                Debug.LogError($"[LevelManager] Prefab '{objectData.id}' has no LevelObject component.");
                return;
            }

            spawnedObjects.Add(levelObject);
            ObjectsRemaining++;
        }

        private IEnumerator ClearCheckRoutine()
        {
            while (true)
            {
                yield return clearCheckWait;

                // The routine starts before the first level, so there is no platform yet and
                // nothing can have fallen off one. Without this the first tick throws and the
                // coroutine dies for good, leaving objects that drop away never cleared.
                if (objectsRoot == null)
                {
                    continue;
                }

                float clearHeight = objectsRoot.position.y - clearDropDistance;

                for (int i = 0; i < spawnedObjects.Count; i++)
                {
                    LevelObject levelObject = spawnedObjects[i];

                    if (levelObject.IsCleared)
                    {
                        continue;
                    }

                    if (levelObject.transform.position.y > clearHeight)
                    {
                        continue;
                    }

                    levelObject.MarkCleared();
                }
            }
        }

        private void HandleObjectCleared(LevelObject levelObject)
        {
            if (!spawnedObjects.Contains(levelObject))
            {
                return;
            }

            ObjectsRemaining--;
            ObjectsRemainingChanged?.Invoke(ObjectsRemaining);

            if (ObjectsRemaining <= 0)
            {
                CompleteLevel();
            }
        }

        private void CompleteLevel()
        {
            StopFailCheck();

            int reward = LevelRules.GetReward(LevelRules.GetLevelType(CurrentLevelNumber));
            EconomyManager.Instance.AddCoins(reward);
            SaveManager.SaveLevel(CurrentLevelNumber + 1);

            Debug.Log($"[LevelManager] Level {CurrentLevelNumber} complete. Reward: {reward} coins.");

            GameManager.Instance.ChangeState(GameState.LevelComplete);
        }

        private void DespawnLevel()
        {
            for (int i = 0; i < spawnedObjects.Count; i++)
            {
                PoolManager.Instance.Despawn(spawnedObjects[i].gameObject);
            }

            spawnedObjects.Clear();
            ObjectsRemaining = 0;
        }

        private void StopFailCheck()
        {
            if (failCheckRoutine == null)
            {
                return;
            }

            StopCoroutine(failCheckRoutine);
            failCheckRoutine = null;
        }

        private IEnumerator FailCheckRoutine()
        {
            yield return settleWait;

            failCheckRoutine = null;

            if (ObjectsRemaining > 0)
            {
                Debug.Log($"[LevelManager] Out of balls. {ObjectsRemaining} objects left.");
                GameManager.Instance.ChangeState(GameState.OutOfBalls);
            }
        }
    }
}
