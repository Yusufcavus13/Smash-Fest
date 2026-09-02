using System;
using UnityEngine;

namespace SmashFest.Levels
{
    /// <summary>
    /// Every layout a level can ask for by name. A level json carries a platformId, and this
    /// is where that name turns into a prefab.
    /// </summary>
    [CreateAssetMenu(fileName = "PlatformLibrary", menuName = "Smash Fest/Platform Library")]
    public class PlatformLibrary : ScriptableObject
    {
        // --- Types ---

        [Serializable]
        public class Entry
        {
            public string id;
            public PlatformPreset prefab;
        }

        // --- Serialized Fields ---

        [Tooltip("Used when a level names a layout that is not in the list.")]
        [SerializeField] private string fallbackId = "platform_static";

        [SerializeField] private Entry[] entries;

        // --- Properties ---

        public string FallbackId => fallbackId;

        // --- Public Methods ---

        public PlatformPreset Get(string id)
        {
            PlatformPreset fallback = null;

            for (int i = 0; i < entries.Length; i++)
            {
                if (entries[i].id == id)
                {
                    return entries[i].prefab;
                }

                if (entries[i].id == fallbackId)
                {
                    fallback = entries[i].prefab;
                }
            }

            Debug.LogWarning($"[PlatformLibrary] No layout named '{id}', falling back to '{fallbackId}'.");

            return fallback;
        }
    }
}
