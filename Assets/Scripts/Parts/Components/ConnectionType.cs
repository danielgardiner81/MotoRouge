using Sirenix.OdinInspector;

namespace Parts.Components
{
    public enum ConnectionType
    {
        [LabelText("Fixed Joint")]
        Fixed,

        [LabelText("Hinge Joint")]
        Hinge,

        [LabelText("Ball Socket Joint")]
        BallSocket
    }
}