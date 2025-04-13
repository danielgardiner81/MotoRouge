using System.Collections.Generic;
using Parts.Components;
using UnityEngine;

namespace Parts.Editor
{
    public static class ConnectionPointGenerator
    {
        public static List<ConnectionPoint> GeneratePoints(ConnectionPreset preset, Bounds bounds)
        {
            var connections = new List<ConnectionPoint>();
            var baseRadius = bounds.extents.magnitude * 0.1f;

            switch (preset)
            {
                case ConnectionPreset.TwoPointsX:
                    connections.AddRange(new[]
                    {
                        CreatePoint("Left", -bounds.extents.x, 0, 0, Vector3.left, baseRadius),
                        CreatePoint("Right", bounds.extents.x, 0, 0, Vector3.right, baseRadius)
                    });
                    break;

                case ConnectionPreset.TwoPointsY:
                    connections.AddRange(new[]
                    {
                        CreatePoint("Bottom", 0, -bounds.extents.y, 0, Vector3.down, baseRadius),
                        CreatePoint("Top", 0, bounds.extents.y, 0, Vector3.up, baseRadius)
                    });
                    break;

                case ConnectionPreset.TwoPointsZ:
                    connections.AddRange(new[]
                    {
                        CreatePoint("Front", 0, 0, -bounds.extents.z, Vector3.back, baseRadius),
                        CreatePoint("Back", 0, 0, bounds.extents.z, Vector3.forward, baseRadius)
                    });
                    break;

                case ConnectionPreset.FourPointsXY:
                    connections.AddRange(new[]
                    {
                        CreatePoint("TopLeft", -bounds.extents.x, bounds.extents.y, 0, (Vector3.up + Vector3.left).normalized, baseRadius),
                        CreatePoint("TopRight", bounds.extents.x, bounds.extents.y, 0, (Vector3.up + Vector3.right).normalized, baseRadius),
                        CreatePoint("BottomLeft", -bounds.extents.x, -bounds.extents.y, 0, (Vector3.down + Vector3.left).normalized, baseRadius),
                        CreatePoint("BottomRight", bounds.extents.x, -bounds.extents.y, 0, (Vector3.down + Vector3.right).normalized, baseRadius)
                    });
                    break;

                case ConnectionPreset.FourPointsXZ:
                    connections.AddRange(new[]
                    {
                        CreatePoint("FrontLeft", -bounds.extents.x, 0, -bounds.extents.z, (Vector3.back + Vector3.left).normalized, baseRadius),
                        CreatePoint("FrontRight", bounds.extents.x, 0, -bounds.extents.z, (Vector3.back + Vector3.right).normalized, baseRadius),
                        CreatePoint("BackLeft", -bounds.extents.x, 0, bounds.extents.z, (Vector3.forward + Vector3.left).normalized, baseRadius),
                        CreatePoint("BackRight", bounds.extents.x, 0, bounds.extents.z, (Vector3.forward + Vector3.right).normalized, baseRadius)
                    });
                    break;

                case ConnectionPreset.CenterPoint:
                    connections.Add(CreatePoint("Center", bounds.center.x, bounds.center.y, bounds.center.z,
                        Vector3.up, baseRadius * 1.5f));
                    break;
            }

            return connections;
        }

        private static ConnectionPoint CreatePoint(string id, float x, float y, float z, Vector3 direction, float radius)
        {
            return new ConnectionPoint
            {
                id = id,
                type = ConnectionType.Fixed,
                localPosition = new Vector3(x, y, z),
                direction = direction,
                radius = radius,
                hingeAxis = Vector3.up,
                hingeLimit = new Vector2(0, 360),
                maxTwistAngle = 60f
            };
        }
    }
}