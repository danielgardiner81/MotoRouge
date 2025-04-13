using Unity.Entities;

namespace Common.Authoring
{
    public class GroundBaker : Baker<GroundAuthoring>
    {
        public override void Bake(GroundAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}