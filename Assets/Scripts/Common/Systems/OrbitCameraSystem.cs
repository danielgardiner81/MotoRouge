using Common.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Common.Systems
{
    public partial class OrbitCameraSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<OrbitCamera>();
            RequireForUpdate<OrbitCameraInput>();
        }

        protected override void OnUpdate()
        {
            foreach (var (camera, input, transform) in
                     SystemAPI.Query<RefRW<OrbitCamera>, RefRO<OrbitCameraInput>, RefRW<LocalTransform>>())
            {
                if (input.ValueRO.IsDragging)
                {
                    // Update orbit angles based on drag
                    var angles = camera.ValueRO.OrbitAngles;
                    angles.y += input.ValueRO.DragDelta.x * camera.ValueRO.RotationSpeed;
                    angles.x -= input.ValueRO.DragDelta.y * camera.ValueRO.RotationSpeed;

                    // Clamp pitch to avoid flipping
                    angles.x = math.clamp(angles.x, -85f, 85f);

                    // Normalize yaw to 0-360
                    angles.y = math.fmod(angles.y, 360f);

                    camera.ValueRW.OrbitAngles = angles;
                }

                // Handle zoom
                if (math.abs(input.ValueRO.ZoomDelta) > 0)
                {
                    camera.ValueRW.Distance = math.clamp(
                        camera.ValueRO.Distance - input.ValueRO.ZoomDelta * camera.ValueRO.ZoomSpeed,
                        camera.ValueRO.MinDistance,
                        camera.ValueRO.MaxDistance
                    );
                }

                // Update position
                var rotation = quaternion.Euler(
                    math.radians(camera.ValueRO.OrbitAngles.x),
                    math.radians(camera.ValueRO.OrbitAngles.y),
                    0f
                );

                transform.ValueRW.Position = camera.ValueRO.FocusPoint + math.rotate(rotation, new float3(0f, 0f, -camera.ValueRO.Distance));
                transform.ValueRW.Rotation = rotation;
            }
        }
    }
}