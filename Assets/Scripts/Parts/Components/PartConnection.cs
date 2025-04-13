using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Parts.Components
{
    [System.Serializable]
    public class PartConnection
    {
        [SerializeField]
        public List<ConnectionPoint> connectionPoints = new();

        public bool ValidateConnections()
        {
            if (connectionPoints == null) return true;

            var ids = new HashSet<string>();
            foreach (var point in connectionPoints)
            {
                if (string.IsNullOrEmpty(point.id) || !ids.Add(point.id))
                    return false;
            }

            return true;
        }
    }
}