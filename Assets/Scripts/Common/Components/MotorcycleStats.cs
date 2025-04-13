using Unity.Entities;

namespace Common.Components
{
    public struct MotorcycleStats : IComponentData
    {
        public float MaxSpeed;
        public float Acceleration;
        public float TurnSpeed;
        public float Mass;
    }
}