using System;
using SmashFest.Levels;
using SmashFest.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace SmashFest.UI
{

    public class SettingsPanel : MonoBehaviour
    {

        [Serializable]
        private class ToggleButton
        {
            public Button button;
            public Image icon;
            public Sprite onSprite;
            public Sprite offSprite;

            public void Apply(bool isOn)
            {
                icon.sprite = isOn ? onSprite : offSprite;
            }
        }

        [Header("References")]
        [Tooltip("Holds the buttons that unfold under the gear.")]
        [SerializeField] private GameObject stackRoot;
        [SerializeField] private Button openButton;
        [SerializeField] private Button exitButton;

        [Header("Toggles")]
        [SerializeField] private ToggleButton music;
        [SerializeField] private ToggleButton sound;
        [SerializeField] private ToggleButton vibration;



        private bool musicEnabled;
        private bool soundEnabled;
        private bool vibrationEnabled;



        private void OnEnable()
        {
            openButton.onClick.AddListener(Toggle);
            exitButton.onClick.AddListener(HandleExitClicked);
            music.button.onClick.AddListener(HandleMusicClicked);
            sound.button.onClick.AddListener(HandleSoundClicked);
            vibration.button.onClick.AddListener(HandleVibrationClicked);
        }

        private void OnDisable()
        {
            openButton.onClick.RemoveListener(Toggle);
            exitButton.onClick.RemoveListener(HandleExitClicked);
            music.button.onClick.RemoveListener(HandleMusicClicked);
            sound.button.onClick.RemoveListener(HandleSoundClicked);
            vibration.button.onClick.RemoveListener(HandleVibrationClicked);

            Time.timeScale = 1f;
        }

        private void Start()
        {
            musicEnabled = SaveManager.LoadMusicEnabled(true);
            soundEnabled = SaveManager.LoadSoundEnabled(true);
            vibrationEnabled = SaveManager.LoadVibrationEnabled(true);

            music.Apply(musicEnabled);
            sound.Apply(soundEnabled);
            vibration.Apply(vibrationEnabled);

            stackRoot.SetActive(false);
        }


        public void Toggle()
        {
            if (stackRoot.activeSelf)
            {
                Close();
                return;
            }

            Open();
        }

        public void Open()
        {
            stackRoot.SetActive(true);
            Time.timeScale = 0f;
        }

        public void Close()
        {
            stackRoot.SetActive(false);
            Time.timeScale = 1f;
        }



        private void HandleMusicClicked()
        {
            musicEnabled = !musicEnabled;
            music.Apply(musicEnabled);
            SaveManager.SaveMusicEnabled(musicEnabled);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicEnabled(musicEnabled);
            }
        }

        private void HandleSoundClicked()
        {
            soundEnabled = !soundEnabled;
            sound.Apply(soundEnabled);
            SaveManager.SaveSoundEnabled(soundEnabled);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSoundEnabled(soundEnabled);
            }
        }

        private void HandleVibrationClicked()
        {
            vibrationEnabled = !vibrationEnabled;
            vibration.Apply(vibrationEnabled);

            // The property writes the setting through, so Haptics never reads a stale value.
            Haptics.Enabled = vibrationEnabled;

            if (vibrationEnabled)
            {
                Haptics.Play();
            }
        }

        private void HandleExitClicked()
        {
            Close();
            LevelManager.Instance.GoHome();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            PanelRootValidator.Validate(this, stackRoot, nameof(stackRoot));
        }
#endif
    }
}
