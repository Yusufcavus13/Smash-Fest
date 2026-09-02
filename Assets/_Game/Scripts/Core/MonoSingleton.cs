using UnityEngine;

namespace SmashFest.Core
{
    
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {

        public static T Instance { get; private set; }


        [Header("Singleton")]
        [SerializeField] private bool persistBetweenScenes;


        private void Awake()
        {
            if (Instance != null)
            {
                Debug.LogWarning($"[{typeof(T).Name}] A second instance was found on '{name}' and destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;

            if (persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnSingletonAwake();
        }

        private void OnDestroy()
        {
            if (Instance != this)
            {
                return;
            }

            Instance = null;
            OnSingletonDestroy();
        }

       
        protected virtual void OnSingletonAwake()
        {
        }

        protected virtual void OnSingletonDestroy()
        {
        }
    }
}
