using Parts.Components;
using Parts.Editor.Drawers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Parts.Editor
{
    public class PartComponent : MonoBehaviour
    {
        [Required]
        public PartDefinition definition;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private Rigidbody rb;

        private void OnEnable()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            if (definition == null) return;

            // Setup mesh
            meshFilter = GetComponent<MeshFilter>();
            if (meshFilter == null) meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = definition.mesh;

            // Setup renderer
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer == null) meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = definition.defaultMaterial;

            // Setup physics
            rb = GetComponent<Rigidbody>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

            rb.mass = definition.mass;
            rb.useGravity = definition.useGravity;
            rb.linearDamping = definition.physics.drag;
            rb.angularDamping = definition.physics.angularDrag;
            rb.isKinematic = false;
            rb.freezeRotation = definition.physics.freezeRotation;

            if (rb.freezeRotation)
            {
                var constraints = RigidbodyConstraints.None;
                rb.constraints = constraints;
            }
        }

        private void OnDrawGizmos()
        {
            if (definition?.connections == null)
                return;

            foreach (var point in definition.connections)
            {
                Gizmos.color = ConnectionPointHelper.GetColorForType(point.type);
                Gizmos.DrawWireSphere(
                    transform.TransformPoint(point.localPosition),
                    point.radius
                );

                // Draw direction
                var start = transform.TransformPoint(point.localPosition);
                var direction = transform.TransformDirection(point.direction.normalized);
                Gizmos.DrawRay(start, direction * point.radius * 2);
            }
        }
    }
}