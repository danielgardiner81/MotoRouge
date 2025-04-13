using System.Collections.Generic;
using System.Linq;
using Parts.Components;
using UnityEngine;

namespace Parts.Editor
{
    public class JointDetectionService
    {
        private readonly Dictionary<string, ConnectionType> _markerTypes = new()
        {
            { "joint_", ConnectionType.Fixed },
            { "hinge_", ConnectionType.Hinge },
            { "ball_", ConnectionType.BallSocket },
            { "socket_", ConnectionType.BallSocket },
            { "connect_", ConnectionType.Fixed }
        };

        private readonly ConnectionPointFactory _pointFactory = new();

        public List<ConnectionPoint> DetectJoints(Mesh mesh)
        {
            var joints = new List<ConnectionPoint>();
            var path = UnityEditor.AssetDatabase.GetAssetPath(mesh);

            foreach (var obj in UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (obj is GameObject go)
                {
                    ProcessHierarchy(go.transform, joints);
                }
            }

            return joints;
        }

        public List<ConnectionPoint> DetectJoints(GameObject prefab)
        {
            var joints = new List<ConnectionPoint>();
            ProcessHierarchy(prefab.transform, joints);
            return joints;
        }

        private void ProcessHierarchy(Transform obj, List<ConnectionPoint> joints)
        {
            var name = obj.name.ToLower();
            var matchingMarker = _markerTypes.FirstOrDefault(m => name.StartsWith((string)m.Key));

            if (!string.IsNullOrEmpty(matchingMarker.Key))
            {
                var point = _pointFactory.CreatePoint(obj, matchingMarker.Value);
                if (point != null)
                {
                    joints.Add(point);
                }
            }

            foreach (Transform child in obj)
            {
                ProcessHierarchy(child, joints);
            }
        }
    }
}