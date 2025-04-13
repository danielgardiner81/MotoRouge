using Unity.Entities;
using Unity.Mathematics;

namespace Common.Components
{
    public struct MotorcycleInput : IComponentData
    {
        public float2 Input;
    }
}