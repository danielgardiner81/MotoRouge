using Sirenix.OdinInspector;

namespace Parts.Components
{
    public enum ConnectionPreset
    {
        [LabelText("2 Points - X Axis")]
        TwoPointsX,

        [LabelText("2 Points - Y Axis")]
        TwoPointsY,

        [LabelText("2 Points - Z Axis")]
        TwoPointsZ,

        [LabelText("4 Points - XY Plane")]
        FourPointsXY,

        [LabelText("4 Points - XZ Plane")]
        FourPointsXZ,

        [LabelText("Center Point")]
        CenterPoint
    }
}