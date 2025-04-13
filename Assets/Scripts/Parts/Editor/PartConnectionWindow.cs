namespace Parts.Editor
{
    // Editor window for part connections
#if UNITY_EDITOR
    public class PartConnectionWindow : UnityEditor.EditorWindow
    {
        [UnityEditor.MenuItem("Motorcycle/Part Connection Editor")]
        public static void ShowWindow()
        {
            GetWindow<PartConnectionWindow>("Part Connections");
        }

        private void OnGUI()
        {
            // Implementation for visual connection editor
        }
    }
#endif
}