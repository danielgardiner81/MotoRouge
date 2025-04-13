using Parts.Components;
using UnityEngine;

namespace Parts.Editor
{
    public class ConnectionPointFactory
    {
        public ConnectionPoint CreatePoint(Transform obj, ConnectionType type)
        {
            var nameParts = obj.name.Split(new[] { '_' }, 2);
            if (nameParts.Length < 2) return null;

            var point = new ConnectionPoint
            {
                id = nameParts[1],
                type = type,
                localPosition = obj.localPosition,
                direction = obj.forward,
                radius = 0.1f
            };

            ConfigurePointByType(point, obj, type);
            return point;
        }

        private void ConfigurePointByType(ConnectionPoint point, Transform obj, ConnectionType type)
        {
            switch (type)
            {
                case ConnectionType.Hinge:
                    ConfigureHingePoint(point, obj);
                    break;

                case ConnectionType.BallSocket:
                    ConfigureBallSocketPoint(point);
                    break;
            }
        }

        private void ConfigureHingePoint(ConnectionPoint point, Transform obj)
        {
            point.hingeAxis = obj.up;
            point.hingeLimit = new Vector2(-45, 45);
        }

        private void ConfigureBallSocketPoint(ConnectionPoint point)
        {
            point.maxTwistAngle = 30f;
        }
    }
}