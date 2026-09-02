using SmashFest.Core;
using UnityEngine;

namespace SmashFest.Systems
{
    /// <summary>
    /// Owns every sound the game makes. Music follows the game state on its own; one-shots
    /// are pushed in by whoever caused them, because only the caller knows what was hit.
    /// </summary>
    public class AudioManager : MonoSingleton<AudioManager>
    {
        // --- Serialized Fields ---

        [Header("References")]
        [SerializeField] private SoundBank bank;
        [SerializeField] private AudioSource musicSource;

        [Header("Voices")]
        [Tooltip("One-shots cycle through these, so overlapping smashes do not cut each other off.")]
        [SerializeField] private int voiceCount = 10;

        [Header("Mix")]
        [Range(0f, 1f)]
        [SerializeField] private float musicVolume = 0.30f;

        [Range(0f, 1f)]
        [SerializeField] private float soundVolume = 0.85f;

        [Tooltip("Random pitch spread on one-shots so repeats do not sound machine made.")]
        [Range(0f, 0.3f)]
        [SerializeField] private float pitchJitter = 0.08f;

        // --- Fields ---

        private AudioSource[] voices;
        private int nextVoice;
        private bool musicEnabled;
        private bool soundEnabled;
        private int lastKnownCoins = -1;

        // --- Unity Messages ---

        private void OnEnable()
        {
            GameManager.StateChanged += HandleStateChanged;
            EconomyManager.CoinsChanged += HandleCoinsChanged;
        }

        private void OnDisable()
        {
            GameManager.StateChanged -= HandleStateChanged;
            EconomyManager.CoinsChanged -= HandleCoinsChanged;
        }

        private void Start()
        {
            // ChangeState ignores a move to the state it is already in, so the opening Home
            // is never broadcast. Every other view reads the current state here too.
            PlayMusicFor(GameManager.Instance.CurrentState);
        }

        // --- Public Methods ---

        /// <summary>
        /// A hit the object survived. <paramref name="strength"/> is 0..1 and drives volume,
        /// so a nudge is quieter than a direct cannon shot.
        /// </summary>
        public void PlayImpact(MaterialType material, float strength)
        {
            SoundBank.MaterialSounds sounds = bank.Get(material);

            if (sounds == null)
            {
                return;
            }

            PlayOneShot(Pick(sounds.impact), Mathf.Clamp01(strength));
        }

        public void PlayShatter(MaterialType material)
        {
            SoundBank.MaterialSounds sounds = bank.Get(material);

            if (sounds == null)
            {
                return;
            }

            PlayOneShot(Pick(sounds.shatter), 1f);
        }

        public void PlayShoot()
        {
            PlayOneShot(bank.ballShoot, 1f);
        }

        public void PlayTap()
        {
            PlayOneShot(bank.buttonTap, 1f);
        }

        public void SetMusicEnabled(bool value)
        {
            musicEnabled = value;
            musicSource.mute = !value;

            if (value && !musicSource.isPlaying && musicSource.clip != null)
            {
                musicSource.Play();
            }
        }

        public void SetSoundEnabled(bool value)
        {
            soundEnabled = value;
        }

        // --- Protected Methods ---

        protected override void OnSingletonAwake()
        {
            voices = new AudioSource[voiceCount];

            for (int i = 0; i < voiceCount; i++)
            {
                AudioSource source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = false;
                voices[i] = source;
            }

            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;

            musicEnabled = SaveManager.LoadMusicEnabled(true);
            soundEnabled = SaveManager.LoadSoundEnabled(true);
            musicSource.mute = !musicEnabled;
        }

        // --- Private Methods ---

        private void HandleStateChanged(GameState state)
        {
            PlayMusicFor(state);

            if (state == GameState.LevelComplete)
            {
                PlayOneShot(bank.levelWin, 1f);
                Haptics.Play();
            }
            else if (state == GameState.LevelFailed)
            {
                PlayOneShot(bank.levelFail, 1f);
                Haptics.Play();
            }
        }

        private void HandleCoinsChanged(int coins)
        {
            // Only a reward should chime; spending is not a celebration.
            bool isReward = lastKnownCoins >= 0 && coins > lastKnownCoins;
            lastKnownCoins = coins;

            if (isReward)
            {
                PlayOneShot(bank.coin, 1f);
            }
        }

        private void PlayMusicFor(GameState state)
        {
            AudioClip wanted = state == GameState.Home ? bank.menuMusic : bank.gameMusic;

            if (wanted == null || musicSource.clip == wanted)
            {
                return;
            }

            musicSource.clip = wanted;

            if (musicEnabled)
            {
                musicSource.Play();
            }
        }

        private void PlayOneShot(AudioClip clip, float volumeScale)
        {
            if (!soundEnabled || clip == null || voices == null)
            {
                return;
            }

            AudioSource source = voices[nextVoice];
            nextVoice = (nextVoice + 1) % voices.Length;

            source.clip = clip;
            source.volume = soundVolume * volumeScale;
            source.pitch = 1f + Random.Range(-pitchJitter, pitchJitter);
            source.Play();
        }

        private static AudioClip Pick(AudioClip[] clips)
        {
            if (clips == null || clips.Length == 0)
            {
                return null;
            }

            return clips[Random.Range(0, clips.Length)];
        }
    }
}
