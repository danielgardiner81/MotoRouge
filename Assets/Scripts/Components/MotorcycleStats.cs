using Unity.Entities;

namespace Components
{
    public struct MotorcycleStats : IComponentData
    {
        public float MaxSpeed;
        public float Acceleration;
        public float TurnSpeed;
        public float Mass;
    }
}