using Unity.Entities;
using Unity.Mathematics;

namespace Authoring
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