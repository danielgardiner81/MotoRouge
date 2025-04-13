using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace Parts.Components
{
    [System.Serializable]
    public class PhysicsProperties
    {
        [BoxGroup("Basic Properties")]
        [MinValue(0)]
        [PropertyTooltip("How quickly velocity decreases")]
        [LabelWidth(50)]
        public float drag = 0.1f;

        [BoxGroup("Basic Properties")]
        [MinValue(0)]
        [PropertyTooltip("How quickly rotation decreases")]
        [LabelWidth(90)]
        public float angularDrag = 0.05f;

        [BoxGroup("Basic Properties")]
        [PropertyTooltip("Physics material for collisions")]
        [InlineEditor(InlineEditorObjectFieldModes.Boxed)]
        [LabelWidth(50)]
        public PhysicsMaterial physicsMaterial;

        [FoldoutGroup("Constraints")]
        [LabelText("Lock Rotation")]
        [PropertyTooltip("Enable to lock rotation on specific axes")]
        [OnValueChanged("OnFreezeRotationChanged")]
        public bool freezeRotation;

        [FoldoutGroup("Constraints")]
        [ShowIf("freezeRotation")]
        [PropertyTooltip("Toggle which axes should have rotation locked")]
        [LabelText("Locked Axes")]
        public Vector3Bool lockedRotationAxes = new();

        private void OnFreezeRotationChanged()
        {
            if (!freezeRotation)
            {
                lockedRotationAxes = new Vector3Bool();
            }
        }

        [Button("Reset to Defaults"), BoxGroup("Basic Properties")]
        [GUIColor(0.3f, 0.8f, 0.3f)]
        private void ResetToDefaults()
        {
            drag = 0.1f;
            angularDrag = 0.05f;
            physicsMaterial = null;
            freezeRotation = false;
            lockedRotationAxes = new Vector3Bool();
        }
    }
}