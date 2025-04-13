using Unity.Entities;
using Unity.Mathematics;

namespace Common.Components
{
    public struct OrbitCameraInput : IComponentData
    {
        public bool IsDragging;
        public float2 DragDelta;
        public float ZoomDelta;
    }
}