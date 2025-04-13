using Parts.Components;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Parts.Editor
{
    [InlineProperty]
    [HideReferenceObjectPicker]
    public class ConnectionPointDisplay
    {
        private ConnectionPoint point;
        private PartDefinition part;

        public ConnectionPointDisplay(ConnectionPoint point, PartDefinition part)
        {
            this.point = point;
            this.part = part;
        }

        [HorizontalGroup("Main")]
        [BoxGroup("Main/Details", false)]
        [HideLabel]
        [InlineProperty]
        [OnValueChanged("OnPointChanged")]
        public ConnectionPoint Point
        {
            get => point;
            set => point = value;
        }

        [BoxGroup("Main/Preview", false)]
        [HorizontalGroup("Main/Preview/Buttons")]
        [Button("Focus", ButtonSizes.Small)]
        private void FocusOnPoint()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                var view = SceneView.lastActiveSceneView;
                view.LookAt(point.localPosition);
            }
        }

        [HorizontalGroup("Main/Preview/Buttons")]
        [Button("Align View", ButtonSizes.Small)]
        private void AlignViewToPoint()
        {
            if (SceneView.lastActiveSceneView != null)
            {
                var view = SceneView.lastActiveSceneView;
                view.rotation = Quaternion.LookRotation(point.direction);
                view.Repaint();
            }
        }

        private void OnPointChanged()
        {
            EditorUtility.SetDirty(part);
        }

        // Handle Scene View visualization
        [OnInspectorGUI]
        private void DrawSceneGUI()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            // Draw connection point handles
            EditorGUI.BeginChangeCheck();

            var newPos = Handles.PositionHandle(point.localPosition, Quaternion.identity);
            var newDir = Handles.RotationHandle(
                Quaternion.LookRotation(point.direction),
                point.localPosition) * Vector3.forward;

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(part, "Modify Connection Point");
                point.localPosition = newPos;
                point.direction = newDir;
                EditorUtility.SetDirty(part);
            }

            // Draw visual representation
            Handles.color = GUI.color;
            Handles.SphereHandleCap(
                0,
                point.localPosition,
                Quaternion.identity,
                point.radius * 2,
                EventType.Repaint
            );

            Handles.DrawLine(
                point.localPosition,
                point.localPosition + point.direction * point.radius * 2
            );

            Handles.Label(point.localPosition + Vector3.up * point.radius * 2, point.id);
        }
    }
}