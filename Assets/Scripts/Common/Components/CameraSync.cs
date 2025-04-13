using Unity.Entities;
using UnityEngine;

namespace Common.Components
{
    public struct CameraSync : IComponentData
    {
        public UnityObjectRef<Camera> CameraReference;
    }
}