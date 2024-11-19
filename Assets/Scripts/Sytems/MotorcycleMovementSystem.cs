using Components;
using Unity.Entities;

namespace Sytems
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

            foreach (var motorcycle in SystemAPI.Query<MotorcycleAspect>())
            {
                motorcycle.Move(deltaTime);
            }
        }
    }
}