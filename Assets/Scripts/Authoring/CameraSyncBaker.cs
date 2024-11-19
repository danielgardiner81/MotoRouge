using Unity.Entities;

namespace Authoring
{
    public class CameraSyncBaker : Baker<CameraSyncAuthoring>
    {
        public override void Bake(CameraSyncAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CameraSync 
            { 
                CameraReference = authoring.unityCamera 
            });
        }
    }
}