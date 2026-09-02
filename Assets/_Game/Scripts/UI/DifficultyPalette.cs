using System;
using SmashFest.Core;
using UnityEngine;

namespace SmashFest.UI
{

    [CreateAssetMenu(fileName = "DifficultyPalette", menuName = "Smash Fest/Difficulty Palette")]
    public class DifficultyPalette : ScriptableObject
    {

        [Serializable]
        public class Skin
        {
            [Tooltip("Body of the retry panel.")]
            public Sprite panel;

            [Tooltip("Level banner across the top of the retry panel.")]
            public Sprite banner;

            [Tooltip("The in game balls counter.")]
            public Sprite badge;

            [Tooltip("The gear button in the corner.")]
            public Sprite settingsButton;

            [Tooltip("Wide buttons, such as Play on the home screen.")]
            public Sprite wideButton;

            [Tooltip("Shown next to the warning icons. Left empty for a normal level.")]
            public string label;
        }


        [SerializeField] private Skin normal;
        [SerializeField] private Skin hard;
        [SerializeField] private Skin superHard;


        public Skin Get(LevelType levelType)
        {
            switch (levelType)
            {
                case LevelType.Hard:
                    return hard;
                case LevelType.SuperHard:
                    return superHard;
                default:
                    return normal;
            }
        }
        public Skin GetForLevel(int levelNumber)
        {
            return Get(LevelRules.GetLevelType(levelNumber));
        }
    }
}
