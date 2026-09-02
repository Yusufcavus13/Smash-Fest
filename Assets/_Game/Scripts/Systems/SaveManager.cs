using UnityEngine;

namespace SmashFest.Systems
{
    public static class SaveManager
    {
        private const string CoinsKey = "smashfest.coins";
        private const string LivesKey = "smashfest.lives";
        private const string LevelKey = "smashfest.level";
        private const string NextLifeKey = "smashfest.nextlife";
        private const string MusicKey = "smashfest.music";
        private const string SoundKey = "smashfest.sound";
        private const string VibrationKey = "smashfest.vibration";

        public static int LoadCoins(int fallback)
        {
            return PlayerPrefs.GetInt(CoinsKey, fallback);
        }

        public static void SaveCoins(int value)
        {
            PlayerPrefs.SetInt(CoinsKey, value);
            PlayerPrefs.Save();
        }

        public static int LoadLives(int fallback)
        {
            return PlayerPrefs.GetInt(LivesKey, fallback);
        }

        public static void SaveLives(int value)
        {
            PlayerPrefs.SetInt(LivesKey, value);
            PlayerPrefs.Save();
        }

        public static int LoadLevel(int fallback)
        {
            return PlayerPrefs.GetInt(LevelKey, fallback);
        }

        public static void SaveLevel(int value)
        {
            PlayerPrefs.SetInt(LevelKey, value);
            PlayerPrefs.Save();
        }

        public static long LoadNextLifeTicks()
        {
            string raw = PlayerPrefs.GetString(NextLifeKey, "0");
            return long.TryParse(raw, out long ticks) ? ticks : 0L;
        }

        public static void SaveNextLifeTicks(long ticks)
        {
            PlayerPrefs.SetString(NextLifeKey, ticks.ToString());
            PlayerPrefs.Save();
        }

        public static bool LoadMusicEnabled(bool fallback)
        {
            return PlayerPrefs.GetInt(MusicKey, fallback ? 1 : 0) == 1;
        }

        public static void SaveMusicEnabled(bool value)
        {
            PlayerPrefs.SetInt(MusicKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool LoadSoundEnabled(bool fallback)
        {
            return PlayerPrefs.GetInt(SoundKey, fallback ? 1 : 0) == 1;
        }

        public static void SaveSoundEnabled(bool value)
        {
            PlayerPrefs.SetInt(SoundKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static bool LoadVibrationEnabled(bool fallback)
        {
            return PlayerPrefs.GetInt(VibrationKey, fallback ? 1 : 0) == 1;
        }

        public static void SaveVibrationEnabled(bool value)
        {
            PlayerPrefs.SetInt(VibrationKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void ResetProgress()
        {
            PlayerPrefs.DeleteKey(CoinsKey);
            PlayerPrefs.DeleteKey(LivesKey);
            PlayerPrefs.DeleteKey(LevelKey);
            PlayerPrefs.DeleteKey(NextLifeKey);
            PlayerPrefs.Save();
        }
    }
}
