using Common.Components;
using Unity.Entities;

namespace Common.Authoring
{
    public class MotorcycleBaker : Baker<MotorcycleAuthoring>
    {
        public override void Bake(MotorcycleAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // Add motorcycle-specific components
            AddComponent(entity, new MotorcycleStats
            {
                MaxSpeed = authoring.maxSpeed,
                Acceleration = authoring.acceleration,
                TurnSpeed = authoring.turnSpeed,
                Mass = authoring.mass
            });

            AddComponent(entity, new MotorcycleInput());
        }
    }
}