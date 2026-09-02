using UnityEngine;

namespace SmashFest.Levels
{
    /// <summary>
    /// Sits on a platform prefab and points at the transform level objects are placed under.
    /// Naming a child "ObjectsRoot" would be enough until someone renames it, so the prefab
    /// says which one it means.
    /// </summary>
    public class PlatformPreset : MonoBehaviour
    {
        // --- Serialized Fields ---

        [Tooltip("Level object positions in the json are relative to this transform.")]
        [SerializeField] private Transform objectsRoot;

        // --- Properties ---

        public Transform ObjectsRoot => objectsRoot;
    }
}
