using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    public struct CameraSync : IComponentData
    {
        public UnityObjectRef<Camera> CameraReference;
    }
}