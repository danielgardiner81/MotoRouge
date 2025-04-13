using System.Collections.Generic;
using Parts.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Parts.Authoring
{
    public class MotorcyclePartManager : MonoBehaviour
    {
        [TabGroup("Parts")]
        [TableList(ShowIndexLabels = true)]
        public List<PartDefinition> availableParts = new();

        [TabGroup("Assembly")]
        [TableList]
        public List<PartInstance> assembledParts = new();

        [TabGroup("Connections")]
        [TableMatrix(SquareCells = true)]
        public bool[,] connectionMatrix = new bool[0,0];

#if UNITY_EDITOR
        [Button("Add New Part")]
        private void AddNewPart()
        {
            var part = ScriptableObject.CreateInstance<PartDefinition>();
            UnityEditor.AssetDatabase.CreateAsset(part, $"Assets/Parts/Part_{availableParts.Count}.asset");
            availableParts.Add(part);
        }
#endif

        [Button("Build Motorcycle")]
        public void BuildMotorcycle()
        {
        }

    }
}