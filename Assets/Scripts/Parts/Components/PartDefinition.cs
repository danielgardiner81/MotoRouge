using System.Collections.Generic;
using Parts.Editor;
using Parts.Editor.Drawers;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Parts.Components
{
    public class PartDefinition : SerializedScriptableObject
    {
        [HorizontalGroup("Split", 0.7f, LabelWidth = 130)]
        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Basic Properties")]
        [Required("Mesh is required"), AssetsOnly]
        [HideLabel]
        [PropertyOrder(1)]
        public Mesh mesh;

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Basic Properties")]
        [Required("Material is required"), AssetsOnly]
        [HideLabel]
        [PropertyOrder(2)]
        public Material defaultMaterial;

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Basic Properties")]
        [TextArea(3, 5)]
        [HideLabel]
        public string description;

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Physics")]
        [MinValue(0.01f)]
        public float mass = 1f;

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Physics")]
        public bool useGravity = true;

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Physics")]
        [Range(0, 1)]
        public float friction = 0.5f;

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Physics")]
        [InlineProperty]
        public PhysicsProperties physics = new();

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Scaling")]
        [InlineProperty]
        public PartScaling scaling = new();

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Connections")]
        [InfoBox("Select a preset to create connection points based on the mesh bounds")]
        [EnumToggleButtons]
        public ConnectionPreset connectionPreset;

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Connections")]
        [Button("Generate Connection Points"), GUIColor(0.3f, 0.8f, 0.3f)]
        private void GenerateConnectionPoints()
        {
            if (mesh == null) return;
            connections = ConnectionPointGenerator.GeneratePoints(connectionPreset, mesh.bounds);
        }

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Connections")]
        [ListDrawerSettings(ShowIndexLabels = true, DraggableItems = true, ShowItemCount = true, ShowFoldout = true, ShowPaging = false)]
        public List<ConnectionPoint> connections = new();

        [VerticalGroup("Split/Left")]
        [TitleGroup("Split/Left/Connections")]
        [Button("Add Connection Point")]
        private void AddConnectionPoint()
        {
            if (connections == null)
                connections = new List<ConnectionPoint>();

            connections.Add(new ConnectionPoint
            {
                id = $"Connection_{connections.Count}",
                type = ConnectionType.Fixed,
                localPosition = Vector3.zero,
                direction = Vector3.up,
                radius = 0.1f,
                hingeAxis = Vector3.up,
                hingeLimit = new Vector2(0, 360),
                maxTwistAngle = 60f
            });
        }

#if UNITY_EDITOR
        private readonly JointDetectionService _jointDetector = new();
        private PartPreviewRenderer _previewRenderer;

        [HorizontalGroup("Split")]
        [VerticalGroup("Split/Right")]
        [OnInspectorGUI]
        private void DrawPreview()
        {
            if (mesh == null) return;

            if (_previewRenderer == null)
            {
                _previewRenderer = new PartPreviewRenderer();
            }

            var rect = GUILayoutUtility.GetRect(0, 600, GUILayout.ExpandWidth(true));
            _previewRenderer.DrawPreview(rect, this);
        }

        [TitleGroup("Connections")]
        [Button("Detect Connection Points"), GUIColor(0.3f, 0.8f, 0.3f)]
        private void DetectConnectionPoints()
        {
            if (!mesh) return;

            var detectedJoints = _jointDetector.DetectJoints(mesh);

            if (detectedJoints.Count > 0)
            {
                if (EditorUtility.DisplayDialog("Connection Points Found",
                        $"Found {detectedJoints.Count} connection points. Add them to existing connections?",
                        "Add", "Replace All"))
                {
                    connections.AddRange(detectedJoints);
                }
                else
                {
                    connections = detectedJoints;
                }
            }
        }

        private void OnDisable()
        {
            if (_previewRenderer != null)
            {
                _previewRenderer.Cleanup();
                _previewRenderer = null;
            }
        }
#endif
    }
}