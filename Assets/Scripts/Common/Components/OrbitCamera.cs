using Unity.Entities;
using Unity.Mathematics;

namespace Common.Components
{
    public struct OrbitCamera : IComponentData
    {
        public float Distance;
        public float MinDistance;
        public float MaxDistance;
        public float ZoomSpeed;
        public float2 OrbitAngles;
        public float3 FocusPoint;
        public float RotationSpeed;
    }
}