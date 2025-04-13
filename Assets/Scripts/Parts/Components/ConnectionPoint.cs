using Sirenix.OdinInspector;
using UnityEngine;

namespace Parts.Components
{
    [System.Serializable]
    public class ConnectionPoint
    {
        [HorizontalGroup("Header")]
        [LabelWidth(50)]
        public string id;

        [HorizontalGroup("Header")]
        [LabelWidth(50)]
        public ConnectionType type;

        [FoldoutGroup("Transform")]
        [LabelText("Position")]
        public Vector3 localPosition;

        [FoldoutGroup("Transform")]
        public Vector3 direction;

        [FoldoutGroup("Transform")]
        [MinValue(0.01f)]
        public float radius = 0.1f;

        [ShowIf("@type == ConnectionType.Hinge")]
        [FoldoutGroup("Joint Settings")]
        public Vector3 hingeAxis = Vector3.up;

        [ShowIf("@type == ConnectionType.Hinge")]
        [FoldoutGroup("Joint Settings")]
        [MinMaxSlider(-180, 180, true)]
        public Vector2 hingeLimit = new(0, 360);

        [ShowIf("@type == ConnectionType.BallSocket")]
        [FoldoutGroup("Joint Settings")]
        [Range(0, 180)]
        public float maxTwistAngle = 60f;
    }
}