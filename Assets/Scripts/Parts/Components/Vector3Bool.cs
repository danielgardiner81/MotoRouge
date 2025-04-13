using Sirenix.OdinInspector;

namespace Parts.Components
{
    [System.Serializable]
    public struct Vector3Bool
    {
        [HorizontalGroup("Axes")]
        [LabelText("X")] public bool x;
        [HorizontalGroup("Axes")]
        [LabelText("Y")] public bool y;
        [HorizontalGroup("Axes")]
        [LabelText("Z")] public bool z;
    }
}