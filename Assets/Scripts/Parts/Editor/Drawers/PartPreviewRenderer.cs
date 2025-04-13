using Parts.Components;
using UnityEditor;
using UnityEngine;

namespace Parts.Editor.Drawers
{
    public class PartPreviewRenderer
    {
        private PreviewRenderUtility _previewUtility;
        private Vector2 _previewRotation;
        private bool _isDragging;
        private Vector2 _lastMousePosition;
        private Mesh _sphereMesh;
        private const float PreviewFOV = 30f;
        private const float PreviewPadding = 1.2f;

        public PartPreviewRenderer()
        {
            InitializePreviewUtility();
        }

        private void InitializePreviewUtility()
        {
            _previewUtility = new PreviewRenderUtility();
            _previewUtility.camera.transform.position = new Vector3(0, 0, -5);
            _previewUtility.camera.transform.rotation = Quaternion.identity;
            _previewUtility.camera.fieldOfView = PreviewFOV;
            _previewUtility.camera.nearClipPlane = 0.01f;
            _previewUtility.camera.farClipPlane = 1000f;
            _previewUtility.ambientColor = new Color(0.2f, 0.2f, 0.2f, 1);
            _previewUtility.lights[0].intensity = 1f;
            _previewUtility.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
        }

        public void DrawPreview(Rect rect, PartDefinition part)
        {
            if (part?.mesh == null || rect.width <= 0 || rect.height <= 0)
                return;

            HandlePreviewInput(rect);

            try
            {
                _previewUtility.BeginPreview(rect, GUIStyle.none);

                DrawMesh(part);
                DrawConnectionPoints(part, rect);

                _previewUtility.camera.Render();
                var texture = _previewUtility.EndPreview();
                GUI.DrawTexture(rect, texture);

                DrawRotationHint(rect);
            }
            catch
            {
                InitializePreviewUtility();
            }
        }

        private Vector3 GetPartScale(PartDefinition part)
        {
            var scale = Vector3.one;

            switch (part.scaling.mode)
            {
                case PartScaling.ScalingMode.Uniform:
                    var uniformScale = Mathf.Clamp(part.scaling.uniformScale,
                        part.scaling.scaleLimit.x,
                        part.scaling.scaleLimit.y);
                    scale = new Vector3(uniformScale, uniformScale, uniformScale);
                    break;

                case PartScaling.ScalingMode.NonUniform:
                    scale = new Vector3(
                        Mathf.Clamp(part.scaling.nonUniformScale.x, part.scaling.scaleLimit.x, part.scaling.scaleLimit.y),
                        Mathf.Clamp(part.scaling.nonUniformScale.y, part.scaling.scaleLimit.x, part.scaling.scaleLimit.y),
                        Mathf.Clamp(part.scaling.nonUniformScale.z, part.scaling.scaleLimit.x, part.scaling.scaleLimit.y)
                    );
                    break;

                case PartScaling.ScalingMode.Computed:
                    // Add computed scaling logic here if needed
                    break;
            }

            return scale;
        }

        private void DrawMesh(PartDefinition part)
        {
            var meshRotation = Quaternion.Euler(_previewRotation.x, _previewRotation.y, 0);
            var previewMaterial = part.defaultMaterial != null ? part.defaultMaterial : new Material(Shader.Find("Standard"));

            // Get scaled bounds
            var scale = GetPartScale(part);
            var bounds = part.mesh.bounds;
            var scaledSize = Vector3.Scale(bounds.size, scale);
            var maxDim = Mathf.Max(scaledSize.x, scaledSize.y, scaledSize.z);

            // Calculate camera distance based on scaled size
            var fov = _previewUtility.camera.fieldOfView * Mathf.Deg2Rad;
            var dist = (maxDim * PreviewPadding) / Mathf.Tan(fov * 0.5f);

            _previewUtility.camera.transform.position = Vector3.back * dist;

            // Apply scaling to the mesh matrix
            var meshMatrix = Matrix4x4.TRS(
                -bounds.center,
                meshRotation,
                scale
            );

            _previewUtility.DrawMesh(part.mesh, meshMatrix, previewMaterial, 0);
        }

        private void DrawConnectionPoints(PartDefinition part, Rect rect)
        {
            if (part.connections == null || part.connections.Count == 0)
                return;

            var meshRotation = Quaternion.Euler(_previewRotation.x, _previewRotation.y, 0);
            var scale = GetPartScale(part);
            var debugMaterial = new Material(Shader.Find("Hidden/Internal-Colored"))
            {
                hideFlags = HideFlags.HideAndDontSave
            };

            foreach (var connection in part.connections)
            {
                // Scale the connection position
                var scaledPosition = Vector3.Scale(connection.localPosition, scale);
                var worldPos = meshRotation * (scaledPosition - Vector3.Scale(part.mesh.bounds.center, scale));
                var direction = meshRotation * connection.direction;

                // Scale the connection radius based on the average scale
                var averageScale = (scale.x + scale.y + scale.z) / 3f;
                var scaledRadius = connection.radius * averageScale;

                // Draw connection sphere
                var sphereMesh = GetSphereMesh();
                var sphereMatrix = Matrix4x4.TRS(worldPos, meshRotation, Vector3.one * scaledRadius);

                debugMaterial.SetColor("_Color", GetConnectionColor(connection.type));
                debugMaterial.SetPass(0);

                _previewUtility.DrawMesh(sphereMesh, sphereMatrix, debugMaterial, 0);

                // Draw direction line
                GL.PushMatrix();
                GL.MultMatrix(_previewUtility.camera.worldToCameraMatrix);
                debugMaterial.SetPass(0);
                GL.Begin(GL.LINES);
                GL.Color(GetConnectionColor(connection.type));
                GL.Vertex(worldPos);
                GL.Vertex(worldPos + direction * (scaledRadius * 2));
                GL.End();
                GL.PopMatrix();

                // Draw connection label
                var screenPos = _previewUtility.camera.WorldToScreenPoint(worldPos);
                if (screenPos.z > 0)
                {
                    var style = new GUIStyle(GUI.skin.label)
                    {
                        normal = { textColor = GetConnectionColor(connection.type) },
                        alignment = TextAnchor.MiddleCenter
                    };

                    var labelRect = new Rect(
                        screenPos.x - 50,
                        rect.height - screenPos.y - 10,
                        100,
                        20
                    );

                    if (labelRect.y > 0 && labelRect.y < rect.height)
                    {
                        GUI.Label(labelRect, connection.id, style);
                    }
                }
            }
        }

        private Mesh GetSphereMesh()
        {
            if (_sphereMesh == null)
            {
                _sphereMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            }

            return _sphereMesh;
        }

        private Color GetConnectionColor(ConnectionType type)
        {
            return type switch
            {
                ConnectionType.Fixed => new Color(0.3f, 0.8f, 1f),
                ConnectionType.Hinge => new Color(1f, 0.8f, 0.3f),
                ConnectionType.BallSocket => new Color(1f, 0.3f, 0.8f),
                _ => Color.white
            };
        }

        private void HandlePreviewInput(Rect rect)
        {
            var e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:
                    if (rect.Contains(e.mousePosition) && e.button == 0)
                    {
                        _isDragging = true;
                        _lastMousePosition = e.mousePosition;
                        e.Use();
                    }

                    break;

                case EventType.MouseUp:
                    _isDragging = false;
                    break;

                case EventType.MouseDrag:
                    if (_isDragging)
                    {
                        _previewRotation.y += (e.mousePosition.x - _lastMousePosition.x) * 0.5f;
                        _previewRotation.x += (e.mousePosition.y - _lastMousePosition.y) * 0.5f;
                        _previewRotation.x = Mathf.Clamp(_previewRotation.x, -90f, 90f);
                        _lastMousePosition = e.mousePosition;
                        e.Use();
                    }

                    break;
            }
        }

        private void DrawRotationHint(Rect rect)
        {
            EditorGUI.LabelField(
                new Rect(rect.x + 5, rect.y + 5, 200, 20),
                "Left click and drag to rotate",
                new GUIStyle(GUI.skin.label) { normal = { textColor = Color.white } }
            );
        }

        public void Cleanup()
        {
            if (_previewUtility != null)
            {
                _previewUtility.Cleanup();
                _previewUtility = null;
            }

            if (_sphereMesh != null)
            {
                _sphereMesh = null;
            }
        }
    }
}