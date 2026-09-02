using UnityEngine;

namespace SmashFest.Levels
{
    /// <summary>
    /// Reads a level json file into a <see cref="LevelData"/>. Static because it owns no
    /// state, it is a pure "give me a number, take a level" service.
    /// </summary>
    public static class LevelLoader
    {

        private const string LevelPathFormat = "Levels/level_{0}";


        /// <summary>
        /// True when a level file with this number is shipped with the game.
        /// </summary>
        public static bool Exists(int levelNumber)
        {
            TextAsset asset = Resources.Load<TextAsset>(string.Format(LevelPathFormat, levelNumber));

            if (asset == null)
            {
                return false;
            }

            Resources.UnloadAsset(asset);

            return true;
        }

        public static LevelData Load(int levelNumber)
        {
            string path = string.Format(LevelPathFormat, levelNumber);
            TextAsset asset = Resources.Load<TextAsset>(path);

            if (asset == null)
            {
                Debug.LogError($"[LevelLoader] No level file at Resources/{path}.json");
                return null;
            }

            LevelData data = JsonUtility.FromJson<LevelData>(asset.text);
            Resources.UnloadAsset(asset);

            if (data == null || data.objects == null || data.objects.Length == 0)
            {
                Debug.LogError($"[LevelLoader] Level {levelNumber} is empty or malformed.");
                return null;
            }

            return data;
        }
    }
}
