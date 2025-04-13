using Common.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Common.Aspect
{
    public readonly partial struct MotorcycleAspect : IAspect
    {
        readonly RefRW<LocalTransform> Transform;
        readonly RefRW<PhysicsVelocity> Velocity;
        readonly RefRO<MotorcycleInput> Input;
        readonly RefRO<MotorcycleStats> Stats;

        public void Move(float deltaTime)
        {
            var radius = 0.5f; // Assuming unit sphere with radius 0.5
        
            // Calculate torque based on input
            var torque = new float3(
                -Input.ValueRO.Input.y, 
                0,
                Input.ValueRO.Input.x
            ) * Stats.ValueRO.TurnSpeed;

            torque *= Stats.ValueRO.Acceleration;
        
            // Only apply torque when there's input
            if (math.lengthsq(new float2(Input.ValueRO.Input.x, Input.ValueRO.Input.y)) > 0.0001f)
            {
                Velocity.ValueRW.Angular += torque * deltaTime;
            
                // Convert angular velocity to linear velocity
                var horizontalVelocity = math.cross(Velocity.ValueRO.Angular, math.up() * radius);
            
                // Apply speed limit if needed
                var speed = math.length(horizontalVelocity);
                if (speed > Stats.ValueRO.MaxSpeed)
                {
                    Velocity.ValueRW.Angular = speed;
                }
            }
        }
    }
}