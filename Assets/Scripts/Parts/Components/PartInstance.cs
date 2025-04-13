using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Parts.Components
{
    [System.Serializable]
    public class PartInstance
    {
        [Required]
        public PartDefinition definition;
        public Vector3 position;
        public Quaternion rotation;
        public Material materialOverride;
        [ReadOnly]
        public List<string> connectedTo = new();
    }
}