using System;
using SmashFest.Levels;
using SmashFest.Systems;
using UnityEditor;
using UnityEngine;

namespace SmashFest.EditorTools
{

    public class DevToolsWindow : EditorWindow
    {
  

        private const string CoinsKey = "smashfest.coins";
        private const string LivesKey = "smashfest.lives";
        private const string LevelKey = "smashfest.level";
        private const string NextLifeKey = "smashfest.nextlife";

        private int coins;
        private int lives;
        private int level;
        private bool loaded;


        private void OnEnable()
        {
            titleContent = new GUIContent("Smash Fest Dev");
            ReadSaved();
        }

        private void OnGUI()
        {
            if (!loaded)
            {
                ReadSaved();
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Saved State", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(DescribeNextLife(), MessageType.None);

            EditorGUILayout.Space(4f);
            coins = EditorGUILayout.IntField("Coins", coins);
            lives = EditorGUILayout.IntField("Lives", lives);
            level = EditorGUILayout.IntField("Level", level);

            EditorGUILayout.Space(8f);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply", GUILayout.Height(26f)))
                {
                    Apply();
                }

                if (GUILayout.Button("Reload from save", GUILayout.Height(26f)))
                {
                    ReadSaved();
                }
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Shortcuts", EditorStyles.boldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Fill lives"))
                {
                    lives = MaxLives();
                    Apply();
                }

                if (GUILayout.Button("Empty lives"))
                {
                    lives = 0;
                    Apply();
                }

                if (GUILayout.Button("+1000 coins"))
                {
                    coins += 1000;
                    Apply();
                }
            }

            EditorGUILayout.Space(10f);
            if (GUILayout.Button("Reset progress"))
            {
                if (EditorUtility.DisplayDialog(
                        "Reset progress",
                        "This clears coins, lives and level progress. There is no undo.",
                        "Reset",
                        "Cancel"))
                {
                    ResetProgress();
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space(6f);
                EditorGUILayout.HelpBox(
                    "The game is not playing, so these are written to the save file and picked "
                    + "up when you press Play.",
                    MessageType.Info);
            }
        }

        // --- Private Methods ---

        [MenuItem("Tools/Smash Fest/Dev Tools")]
        private static void Open()
        {
            GetWindow<DevToolsWindow>();
        }

        private void ReadSaved()
        {
            coins = PlayerPrefs.GetInt(CoinsKey, 1000);
            lives = PlayerPrefs.GetInt(LivesKey, MaxLives());
            level = PlayerPrefs.GetInt(LevelKey, 1);
            loaded = true;
            Repaint();
        }

        private void Apply()
        {
            lives = Mathf.Clamp(lives, 0, MaxLives());
            coins = Mathf.Max(0, coins);
            level = Mathf.Max(1, level);

            EconomyManager economy = LiveEconomy();

            if (economy != null)
            {
                // Goes through the manager so the HUD updates and the timer stays consistent.
                economy.EditorApplyWallet(coins, lives);
            }
            else
            {
                PlayerPrefs.SetInt(CoinsKey, coins);
                PlayerPrefs.SetInt(LivesKey, lives);
                PlayerPrefs.SetString(NextLifeKey, "0");
            }

            PlayerPrefs.SetInt(LevelKey, level);
            PlayerPrefs.Save();

            if (Application.isPlaying && LevelManager.Instance != null
                && LevelManager.Instance.CurrentLevelNumber != level)
            {
                LevelManager.Instance.LoadLevel(level);
            }

            Repaint();
        }

        private void ResetProgress()
        {
            EconomyManager economy = LiveEconomy();

            if (economy != null)
            {
                economy.ResetProgress();
            }
            else
            {
                SaveManager.ResetProgress();
            }

            ReadSaved();
        }

        private string DescribeNextLife()
        {
            string raw = PlayerPrefs.GetString(NextLifeKey, "0");

            if (!long.TryParse(raw, out long ticks) || ticks == 0L)
            {
                return "No regen timer running.";
            }

            TimeSpan remaining = new DateTime(ticks) - DateTime.UtcNow;

            return remaining.Ticks <= 0L
                ? "A life is ready to be collected."
                : $"Next life in {remaining:hh\\:mm\\:ss}.";
        }

        private static EconomyManager LiveEconomy()
        {
            return Application.isPlaying
                ? FindFirstObjectByType<EconomyManager>()
                : null;
        }

        private static int MaxLives()
        {
            EconomyManager economy = FindFirstObjectByType<EconomyManager>();

            return economy != null ? economy.MaxLives : 5;
        }
    }
}

