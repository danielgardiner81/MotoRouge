using Common.Components;
using Unity.Entities;

namespace Common.Authoring
{
    public class OrbitCameraBaker : Baker<OrbitCameraAuthoring>
    {
        public override void Bake(OrbitCameraAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
        
            AddComponent(entity, new OrbitCamera
            {
                Distance = authoring.distance,
                OrbitAngles = authoring.initialAngles,
                FocusPoint = authoring.focusPoint,
                RotationSpeed = authoring.rotationSpeed,
                MinDistance = authoring.minDistance,
                MaxDistance = authoring.maxDistance,     
                ZoomSpeed = authoring.zoomSpeed  
            });
        
            AddComponent(entity, new OrbitCameraInput());
        }
    }
}