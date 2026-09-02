using System;
using System.Collections.Generic;
using SmashFest.Core;
using UnityEngine;

namespace SmashFest.Systems
{
    /// <summary>
    /// Keeps a ready to use set of instances for every prefab in the game, so nothing is
    /// created or destroyed while the player is shooting. Every pooled prefab is addressed
    /// by a string id, which is also the id used inside the level json files.
    /// </summary>
    public class PoolManager : MonoSingleton<PoolManager>
    {
        // --- Constants ---

        private const int MaxPrewarmCount = 200;

        // --- Types ---

        [Serializable]
        private class PoolDefinition
        {
            public string id;
            public GameObject prefab;
            public int prewarmCount = 10;
        }

        // --- Serialized Fields ---

        [Header("Pools")]
        [SerializeField] private PoolDefinition[] definitions;

        // --- Fields ---

        private readonly Dictionary<string, Queue<GameObject>> availableById = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, GameObject> prefabById = new Dictionary<string, GameObject>();
        private readonly Dictionary<string, Transform> rootById = new Dictionary<string, Transform>();
        private readonly Dictionary<GameObject, string> idByInstance = new Dictionary<GameObject, string>();

        // --- Protected Methods ---

        protected override void OnSingletonAwake()
        {
            BuildPools();
        }

        // --- Public Methods ---

        public GameObject Spawn(string id, Vector3 position, Quaternion rotation)
        {
            if (!availableById.TryGetValue(id, out Queue<GameObject> available))
            {
                Debug.LogError($"[PoolManager] Unknown pool id '{id}'.");
                return null;
            }

            GameObject instance = available.Count > 0 ? available.Dequeue() : CreateInstance(id);

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetActive(true);

            return instance;
        }

        public void Despawn(GameObject instance)
        {
            if (!instance.activeSelf)
            {
                return;
            }

            instance.SetActive(false);

            if (!idByInstance.TryGetValue(instance, out string id))
            {
                Debug.LogWarning($"[PoolManager] '{instance.name}' was not spawned by the pool.");
                return;
            }

            instance.transform.SetParent(rootById[id], false);
            availableById[id].Enqueue(instance);
        }

        // --- Private Methods ---

        private void BuildPools()
        {
            for (int i = 0; i < definitions.Length; i++)
            {
                PoolDefinition definition = definitions[i];

                if (!IsDefinitionValid(definition))
                {
                    continue;
                }

                Transform root = new GameObject($"Pool_{definition.id}").transform;
                root.SetParent(transform, false);

                prefabById.Add(definition.id, definition.prefab);
                rootById.Add(definition.id, root);

                int prewarmCount = Mathf.Clamp(definition.prewarmCount, 0, MaxPrewarmCount);
                Queue<GameObject> available = new Queue<GameObject>(prewarmCount);
                availableById.Add(definition.id, available);

                for (int j = 0; j < prewarmCount; j++)
                {
                    available.Enqueue(CreateInstance(definition.id));
                }
            }
        }

        /// <summary>
        /// Guards against setup mistakes in the inspector. A bad definition is skipped with a
        /// clear message instead of taking the editor down with it.
        /// </summary>
        private bool IsDefinitionValid(PoolDefinition definition)
        {
            if (string.IsNullOrEmpty(definition.id))
            {
                Debug.LogError("[PoolManager] A pool definition has an empty id.");
                return false;
            }

            if (definition.prefab == null)
            {
                Debug.LogError($"[PoolManager] Pool '{definition.id}' has no prefab assigned.");
                return false;
            }

            if (availableById.ContainsKey(definition.id))
            {
                Debug.LogError($"[PoolManager] Pool id '{definition.id}' is declared more than once.");
                return false;
            }

            if (definition.prefab.GetComponentInChildren<PoolManager>(true) != null)
            {
                Debug.LogError($"[PoolManager] Prefab '{definition.prefab.name}' contains a PoolManager. " +
                    "A manager must never live inside a pooled prefab, it would copy itself endlessly.");
                return false;
            }

            return true;
        }

        private GameObject CreateInstance(string id)
        {
            GameObject instance = Instantiate(prefabById[id], rootById[id]);
            instance.SetActive(false);
            idByInstance.Add(instance, id);

            return instance;
        }
    }
}
