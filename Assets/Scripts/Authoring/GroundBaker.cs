using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Authoring
{
    public class GroundBaker : Baker<GroundAuthoring>
    {
        public override void Bake(GroundAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}