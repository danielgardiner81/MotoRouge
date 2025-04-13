using Parts.Components;
using UnityEngine;

namespace Parts.Editor.Drawers
{
    public static class ConnectionPointHelper
    {
        public static Color GetColorForType(ConnectionType type)
        {
            return type switch
            {
                ConnectionType.Fixed => new Color(0.3f, 0.5f, 1f),
                ConnectionType.Hinge => new Color(0.3f, 1f, 0.5f),
                ConnectionType.BallSocket => new Color(1f, 0.8f, 0.3f),
                _ => Color.white
            };
        }

        public static bool CanConnect(ConnectionPoint a, ConnectionPoint b)
        {
            if (a == null || b == null) return false;

            // Check compatibility matrix
            switch (a.type)
            {
                case ConnectionType.Fixed:
                    return b.type == ConnectionType.Fixed;

                case ConnectionType.Hinge:
                    return b.type == ConnectionType.Fixed || b.type == ConnectionType.Hinge;

                case ConnectionType.BallSocket:
                    return b.type == ConnectionType.Fixed || b.type == ConnectionType.BallSocket;

                default:
                    return false;
            }
        }
    }
}