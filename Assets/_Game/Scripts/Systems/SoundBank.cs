using System;
using SmashFest.Core;
using UnityEngine;

namespace SmashFest.Systems
{
    /// <summary>
    /// Every clip the game can play, in one asset. Impacts and breaks are looked up by
    /// material so a glass jar never sounds like a crate.
    /// </summary>
    [CreateAssetMenu(fileName = "SoundBank", menuName = "Smash Fest/Sound Bank")]
    public class SoundBank : ScriptableObject
    {
        // --- Types ---

        [Serializable]
        public class MaterialSounds
        {
            public MaterialType material;

            [Tooltip("Played when the object is struck but survives. One is picked at random.")]
            public AudioClip[] impact;

            [Tooltip("Played when the object breaks apart.")]
            public AudioClip[] shatter;
        }

        // --- Serialized Fields ---

        [Header("Music")]
        public AudioClip menuMusic;
        public AudioClip gameMusic;

        [Header("Interface")]
        public AudioClip buttonTap;
        public AudioClip coin;

        [Header("Gameplay")]
        public AudioClip ballShoot;
        public AudioClip levelWin;
        public AudioClip levelFail;

        [Header("Materials")]
        [SerializeField] private MaterialSounds[] materials;

        // --- Public Methods ---

        public MaterialSounds Get(MaterialType material)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].material == material)
                {
                    return materials[i];
                }
            }

            return materials.Length > 0 ? materials[0] : null;
        }
    }
}
