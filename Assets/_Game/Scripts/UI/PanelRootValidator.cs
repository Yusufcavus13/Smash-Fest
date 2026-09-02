using UnityEngine;

namespace SmashFest.UI
{
    /// <summary>
    /// Editor time guard for the "root" fields the ui scripts switch on and off.
    /// Assigning a parent of the script itself silently kills the whole ui, so it is
    /// caught here instead of at runtime.
    /// </summary>
    public static class PanelRootValidator
    {
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Validate(MonoBehaviour owner, GameObject root, string fieldName)
        {
            if (root == null)
            {
                return;
            }

            if (owner.transform.IsChildOf(root.transform))
            {
                Debug.LogError(
                    $"[{owner.GetType().Name}] '{fieldName}' is set to '{root.name}', which contains this script. " +
                    "Switching it off would disable the script itself. Assign the panel object instead.",
                    owner);
            }
        }
    }
}
