using Common.Components;
using Unity.Entities;
using Unity.Transforms;

namespace Common.Systems
{
    public partial class CameraSyncSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            foreach (var (transform, sync) in SystemAPI.Query<RefRO<LocalToWorld>, RefRO<CameraSync>>())
            {
                if (sync.ValueRO.CameraReference.IsValid())
                {
                    var camera = sync.ValueRO.CameraReference.Value;
                    camera.transform.position = transform.ValueRO.Position;
                    camera.transform.rotation = transform.ValueRO.Rotation;
                }
            }
        }
    }
}