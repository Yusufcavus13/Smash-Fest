using UnityEngine;

namespace SmashFest.Systems
{
    /// <summary>
    /// The one place that buzzes the device. <c>Handheld.Vibrate</c> is a single coarse
    /// pulse with no length control, so calling it on every smash would be unbearable —
    /// everything here is throttled and only the meaty moments get through.
    /// </summary>
    public static class Haptics
    {
        // --- Constants ---

        private const float MinInterval = 0.14f;

        // --- Fields ---

        private static bool? isEnabled;
        private static float lastPlayTime = -99f;

        // --- Properties ---

        public static bool Enabled
        {
            get
            {
                if (!isEnabled.HasValue)
                {
                    isEnabled = SaveManager.LoadVibrationEnabled(true);
                }

                return isEnabled.Value;
            }
            set
            {
                isEnabled = value;
                SaveManager.SaveVibrationEnabled(value);
            }
        }

        // --- Public Methods ---

        public static void Play()
        {
            if (!Enabled)
            {
                return;
            }

            if (Time.unscaledTime - lastPlayTime < MinInterval)
            {
                return;
            }

            lastPlayTime = Time.unscaledTime;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }
    }
}
