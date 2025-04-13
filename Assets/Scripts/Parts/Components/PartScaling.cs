using Sirenix.OdinInspector;
using UnityEngine;

namespace Parts.Components
{
    [System.Serializable]
    public class PartScaling
    {
        [EnumToggleButtons]
        public ScalingMode mode = ScalingMode.Uniform;

        [ShowIf("mode", ScalingMode.Uniform)]
        public float uniformScale = 1f;

        [ShowIf("mode", ScalingMode.NonUniform)]
        public Vector3 nonUniformScale = Vector3.one;

        [MinMaxSlider(0.1f, 10f)]
        public Vector2 scaleLimit = new Vector2(0.1f, 5f);

        public enum ScalingMode
        {
            Uniform,
            NonUniform,
            Computed
        }
    }
}