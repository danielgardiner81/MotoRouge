using Parts.Components;
using UnityEditor;
using UnityEngine;

namespace Parts.Editor
{
    public class SceneConnectionPlacer
    {
        private static PartDefinition targetPart;
        private static ConnectionType connectionType;
        private static bool isPlacing;
        private static Material previewMaterial;

        public static void StartPlacement(PartDefinition part, ConnectionType type)
        {
            targetPart = part;
            connectionType = type;
            isPlacing = true;

            // Create preview material if needed
            if (previewMaterial == null)
            {
                previewMaterial = new Material(Shader.Find("Standard"));
                previewMaterial.color = new Color(0, 0.5f, 1f, 0.5f);
            }

            SceneView.duringSceneGui += OnSceneGUI;
            SceneView.RepaintAll();
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!isPlacing || targetPart == null)
                return;

            // Draw mesh preview at origin
            if (targetPart.mesh != null)
            {
                // Draw semi-transparent mesh
                Handles.color = Color.blue;
                Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                Graphics.DrawMesh(
                    targetPart.mesh,
                    Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one),
                    previewMaterial,
                    0,
                    sceneView.camera
                );
            }

            // Handle input
            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                // Get point under mouse
                var ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                var plane = new Plane(Vector3.up, Vector3.zero);

                if (plane.Raycast(ray, out float distance))
                {
                    var worldPoint = ray.GetPoint(distance);

                    // Show a preview of where the connection will be placed
                    Handles.color = GetColorForConnectionType(connectionType);
                    Handles.SphereHandleCap(
                        0,
                        worldPoint,
                        Quaternion.identity,
                        0.1f,
                        EventType.Repaint
                    );

                    /*
                    // Add connection at this point
                    var window = EditorWindow.GetWindow<MotorcyclePartEditor>();
                    window.AddConnection(connectionType, worldPoint);
                    */

                    // Stop placing
                    StopPlacement();
                    e.Use();
                }
            }
            else if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                StopPlacement();
                e.Use();
            }

            sceneView.Repaint();
        }

        private static Color GetColorForConnectionType(ConnectionType type)
        {
            return type switch
            {
                ConnectionType.Fixed => Color.blue,
                ConnectionType.Hinge => Color.green,
                ConnectionType.BallSocket => Color.yellow,
                _ => Color.white
            };
        }

        private static void StopPlacement()
        {
            isPlacing = false;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.RepaintAll();
        }

        private static void CleanUp()
        {
            if (previewMaterial != null)
            {
                Object.DestroyImmediate(previewMaterial);
                previewMaterial = null;
            }
        }
    }
}