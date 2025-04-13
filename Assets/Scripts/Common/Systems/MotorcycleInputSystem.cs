using Common.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Common.Systems
{
    public partial struct MotorcycleInputSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var input in SystemAPI.Query<RefRW<MotorcycleInput>>())
            {
                input.ValueRW.Input = new float2(
                    Input.GetAxis("Horizontal"),
                    Input.GetAxis("Vertical")
                );
            }
        }
    }
}