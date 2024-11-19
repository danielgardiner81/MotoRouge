using UnityEngine;

namespace Authoring
{
    public class OrbitCameraAuthoring : MonoBehaviour
    {
        public float distance = 10f;
        public float rotationSpeed = 3f;
        public Vector3 focusPoint = Vector3.zero;
        public Vector2 initialAngles = new(45f, 45f);
        public float minDistance = 2f;
        public float maxDistance = 20f;
        public float zoomSpeed = 2f;
    }
}