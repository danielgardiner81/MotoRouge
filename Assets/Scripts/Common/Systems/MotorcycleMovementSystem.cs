using Common.Components;
using Unity.Entities;

namespace Common.Systems
{
    public partial struct MotorcycleMovementSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<MotorcycleInput>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var motorcycle in SystemAPI.Query<Common.Aspect.MotorcycleAspect>())
            {
                motorcycle.Move(deltaTime);
            }
        }
    }
}