using UnityEditor;
using UnityEngine;

namespace Parts.Components
{
    public static class EditorIcons
    {
        private static Texture2D locked;
        private static Texture2D unlocked;

        public static Texture2D Locked
        {
            get
            {
                if (locked == null)
                    locked = EditorGUIUtility.IconContent("LockIcon-On").image as Texture2D;
                return locked;
            }
        }

        public static Texture2D Unlocked
        {
            get
            {
                if (unlocked == null)
                    unlocked = EditorGUIUtility.IconContent("LockIcon").image as Texture2D;
                return unlocked;
            }
        }
    }
}