using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using Material = UnityEngine.Material;

namespace Common.Authoring
{
    public class MotorcycleBuilder : MonoBehaviour
    {
        [TabGroup("Meshes")]
        [Required("Wheel mesh is required"), AssetsOnly]
        public Mesh wheelMesh;

        [TabGroup("Meshes")]
        [Required("Frame mesh is required"), AssetsOnly]
        public Mesh frameMesh;

        [TabGroup("Meshes")]
        [Required("Fork mesh is required"), AssetsOnly]
        public Mesh forkMesh;

        [TabGroup("Meshes")]
        [Required("Axle mesh is required"), AssetsOnly]
        public Mesh axleMesh;

        [TabGroup("Materials")]
        [Required("Wheel material is required"), AssetsOnly]
        public Material wheelMaterial;

        [TabGroup("Materials")]
        [Required("Frame material is required"), AssetsOnly]
        public Material frameMaterial;

        [TabGroup("Materials")]
        [Required("Fork material is required"), AssetsOnly]
        public Material forkMaterial;

        [TabGroup("Materials")]
        [Required("Axle material is required"), AssetsOnly]
        public Material axleMaterial;

        [FoldoutGroup("Dimensions/Base")]
        [Title("Reference Dimensions")]
        [Tooltip("Base unit for scaling other parts"), MinValue(0.1f)]
        public float baseUnitSize = 1f;

        [FoldoutGroup("Dimensions/Wheel")]
        [Title("Wheel Dimensions")]
        [PropertyRange(0.3f, 1.5f), LabelText("Wheel Diameter")]
        public float wheelDiameterMultiplier = 0.6f;

        [FoldoutGroup("Dimensions/Wheel")]
        [PropertyRange(0.05f, 0.3f), LabelText("Wheel Thickness")]
        public float wheelThicknessMultiplier = 0.08f;

        [FoldoutGroup("Dimensions/Frame")]
        [Title("Frame Dimensions")]
        [PropertyRange(0.5f, 3f), LabelText("Frame Length")]
        public float frameLengthMultiplier = 1.5f;

        [FoldoutGroup("Dimensions/Frame")]
        [PropertyRange(0.5f, 2f), LabelText("Frame Height")]
        public float frameHeightMultiplier = 0.8f;

        [FoldoutGroup("Dimensions/Frame")]
        [PropertyRange(0.1f, 1f), LabelText("Frame Width")]
        public float frameWidthMultiplier = 0.2f;

        [FoldoutGroup("Dimensions/Forks")]
        [Title("Fork Dimensions")]
        [PropertyRange(0.02f, 0.2f), LabelText("Fork Thickness")]
        public float forkThicknessMultiplier = 0.04f;

        [FoldoutGroup("Dimensions/Forks")]
        [PropertyRange(0.5f, 2f), LabelText("Fork Length")]
        public float forkLengthMultiplier = 0.8f;

        [FoldoutGroup("Dimensions/Axles")]
        [Title("Axle Dimensions")]
        [PropertyRange(0.02f, 0.2f), LabelText("Axle Thickness")]
        public float axleThicknessMultiplier = 0.05f;

        [FoldoutGroup("Dimensions/Axles")]
        [PropertyRange(0.5f, 2f), LabelText("Axle Length")]
        public float axleLengthMultiplier = 1.2f;

        [FoldoutGroup("Physics")]
        [Header("Mass Settings")]
        [MinValue(0.1f)]
        public float wheelMass = 10f;

        [FoldoutGroup("Physics")]
        [MinValue(0.1f)]
        public float frameMass = 150f;

        [FoldoutGroup("Physics")]
        [Header("Suspension Settings")]
        public bool useSuspension = true;

        [FoldoutGroup("Physics"), ShowIf("useSuspension")]
        [MinValue(0)]
        public float suspensionTravel = 0.3f;

        [FoldoutGroup("Physics"), ShowIf("useSuspension")]
        [MinValue(0)]
        public float suspensionStiffness = 50000f;

        [FoldoutGroup("Physics"), ShowIf("useSuspension")]
        [MinValue(0)]
        public float suspensionDamping = 4000f;

        private GameObject bikeRoot;

        [Button(ButtonSizes.Large), GUIColor(0, 1, 0)]
        [HorizontalGroup("Actions")]
        [InfoBox("Creates a new motorcycle with the current settings")]
        public void BuildMotorcycle()
        {
            if (!ValidateSetup()) return;

            if (bikeRoot != null)
                DestroyImmediate(bikeRoot);

            bikeRoot = new GameObject("Motorcycle");
            bikeRoot.transform.SetParent(transform);

            var frame = BuildFrame();
            var frontWheel = BuildWheel("FrontWheel", true);
            var rearWheel = BuildWheel("RearWheel", false);
            var frontForks = BuildForks("Front", true);
            var rearForks = BuildForks("Rear", false);

            SetupPhysicsJoints(frame, frontWheel, rearWheel, frontForks, rearForks);
        }

        private WheelDimensions CalculateWheelDimensions(bool isFront)
        {
            var diameter = baseUnitSize * wheelDiameterMultiplier;
            var thickness = baseUnitSize * wheelThicknessMultiplier;
            var frameLength = baseUnitSize * frameLengthMultiplier;
            var zPos = isFront ? frameLength * 0.4f : -frameLength * 0.4f;

            return new WheelDimensions
            {
                diameter = diameter,
                thickness = thickness,
                position = new Vector3(0, diameter * 0.5f, zPos)
            };
        }

        private GameObject BuildWheel(string name, bool isFront)
        {
            var wheel = new GameObject(name);
            wheel.transform.SetParent(bikeRoot.transform);

            var dims = CalculateWheelDimensions(isFront);

            var wheelObj = CreateMeshObject($"{name}Mesh", wheelMesh, wheelMaterial);
            wheelObj.transform.SetParent(wheel.transform);

            // Make wheel perfectly round by using same value for X and Y
            wheelObj.transform.localScale = new Vector3(
                dims.diameter,
                dims.thickness,
                dims.diameter
            );

            // Correct orientation for a wheel (rotate around X to make it vertical)
            wheelObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            wheel.transform.localPosition = dims.position;

            var physicsBody = wheel.AddComponent<PhysicsBodyAuthoring>();
            physicsBody.Mass = wheelMass;

            var collider = wheel.AddComponent<PhysicsShapeAuthoring>();
            var cylinderGeometry = new CylinderGeometry()
            {
                Height = dims.thickness,
                Radius = dims.diameter * 0.5f,
                Orientation = quaternion.Euler(0, 0, 90),
                Center = float3.zero
            };
            collider.SetCylinder(cylinderGeometry);

            return wheel;
        }

        private Vector3 CalculateAxlePosition(WheelDimensions wheelDims)
        {
            return wheelDims.position;
        }

        private Vector3 CalculateAxleScale(WheelDimensions wheelDims)
        {
            var axleLength = wheelDims.thickness * 2.5f; // Axle extends beyond wheel
            var axleThickness = wheelDims.thickness * 0.2f; // Proportional to wheel

            return new Vector3(axleThickness, axleLength, axleThickness);
        }

        private Vector3[] CalculateForkAttachPoints(WheelDimensions wheelDims, Vector3 axlePos)
        {
            var forkOffset = wheelDims.thickness * 1.2f; // Slightly wider than wheel
            var height = baseUnitSize * frameHeightMultiplier;

            return new Vector3[]
            {
                new Vector3(-forkOffset, height, axlePos.z),
                new Vector3(forkOffset, height, axlePos.z)
            };
        }

        private GameObject BuildForks(string prefix, bool isFront)
        {
            var forkRoot = new GameObject($"{prefix}Forks");
            forkRoot.transform.SetParent(bikeRoot.transform);

            var wheelDims = CalculateWheelDimensions(isFront);
            var axlePos = CalculateAxlePosition(wheelDims);
            var axleScale = CalculateAxleScale(wheelDims);
            var forkAttachPoints = CalculateForkAttachPoints(wheelDims, axlePos);

            float angle = isFront ? -30 : 30;
            var forkThickness = baseUnitSize * forkThicknessMultiplier;
            var forkLength = Vector3.Distance(forkAttachPoints[0], axlePos);

            // Build forks
            for (var i = 0; i < 2; i++)
            {
                var fork = CreateMeshObject($"{prefix}Fork{(i == 0 ? "Left" : "Right")}", forkMesh, forkMaterial);
                fork.transform.SetParent(forkRoot.transform);

                // Position at attach point
                fork.transform.position = forkAttachPoints[i];

                // Calculate rotation to point at axle
                var directionToAxle = (axlePos - forkAttachPoints[i]).normalized;
                fork.transform.rotation = Quaternion.LookRotation(directionToAxle) * Quaternion.Euler(90, 0, 0);

                fork.transform.localScale = new Vector3(forkThickness, forkLength, forkThickness);
            }

            // Build axle
            var axle = CreateMeshObject($"{prefix}Axle", axleMesh, axleMaterial);
            axle.transform.SetParent(forkRoot.transform);
            axle.transform.position = axlePos;
            axle.transform.localRotation = Quaternion.Euler(0, 0, 90);
            axle.transform.localScale = axleScale;

            return forkRoot;
        }

        private GameObject BuildFrame()
        {
            var frame = new GameObject("Frame");
            frame.transform.SetParent(bikeRoot.transform);

            // Get dimensions for front and rear to connect frame
            var frontWheel = CalculateWheelDimensions(true);
            var rearWheel = CalculateWheelDimensions(false);

            var frameHeight = baseUnitSize * frameHeightMultiplier;
            var frameWidth = baseUnitSize * frameWidthMultiplier;

            // Position frame between wheels
            var frameCenter = Vector3.Lerp(frontWheel.position, rearWheel.position, 0.5f);
            frameCenter.y += frameHeight * 0.5f;

            var frameObj = CreateMeshObject("FrameMesh", frameMesh, frameMaterial);
            frameObj.transform.SetParent(frame.transform);
            frameObj.transform.position = frameCenter;

            // Scale frame to connect wheel positions
            var frameLength = Vector3.Distance(frontWheel.position, rearWheel.position);
            frameObj.transform.localScale = new Vector3(frameWidth, frameHeight, frameLength);

            var physicsBody = frame.AddComponent<PhysicsBodyAuthoring>();
            physicsBody.Mass = frameMass;

            var collider = frame.AddComponent<PhysicsShapeAuthoring>();
            var boxGeometry = new BoxGeometry()
            {
                Size = new float3(frameWidth, frameHeight, frameLength),
                Orientation = quaternion.identity,
                Center = float3.zero
            };
            collider.SetBox(boxGeometry);

            return frame;
        }

        private GameObject CreateMeshObject(string name, Mesh mesh, Material material)
        {
            var obj = new GameObject(name);
            var meshFilter = obj.AddComponent<MeshFilter>();
            var meshRenderer = obj.AddComponent<MeshRenderer>();

            meshFilter.sharedMesh = mesh;
            meshRenderer.sharedMaterial = material;

            return obj;
        }

        private void SetupPhysicsJoints(GameObject frame, GameObject frontWheel, GameObject rearWheel,
            GameObject frontForks, GameObject rearForks)
        {
            // Front wheel to fork joint
            var frontWheelJoint = frontWheel.AddComponent<BallAndSocketJoint>();
            frontWheelJoint.ConnectedBody = frontForks.AddComponent<PhysicsBodyAuthoring>();
            frontWheelJoint.AutoSetConnected = true;

            // Rear wheel to fork joint
            var rearWheelJoint = rearWheel.AddComponent<BallAndSocketJoint>();
            rearWheelJoint.ConnectedBody = rearForks.AddComponent<PhysicsBodyAuthoring>();
            rearWheelJoint.AutoSetConnected = true;

            // Fork to frame joints
            var frontForkJoint = frontForks.AddComponent<BallAndSocketJoint>();
            frontForkJoint.ConnectedBody = frame.GetComponent<PhysicsBodyAuthoring>();
            frontForkJoint.AutoSetConnected = true;

            var rearForkJoint = rearForks.AddComponent<BallAndSocketJoint>();
            rearForkJoint.ConnectedBody = frame.GetComponent<PhysicsBodyAuthoring>();
            rearForkJoint.AutoSetConnected = true;

            if (useSuspension)
            {
                AddSuspension(frontWheel, true, frame);
                AddSuspension(rearWheel, false, frame);
            }
        }

        private void AddSuspension(GameObject wheel, bool isFront, GameObject frame)
        {
            var spring = wheel.AddComponent<SpringJoint>();
            spring.connectedBody = frame.GetComponent<Rigidbody>();
            spring.spring = suspensionStiffness;
            spring.damper = suspensionDamping;
            spring.minDistance = 0;
            spring.maxDistance = suspensionTravel;
        }

        [Button(ButtonSizes.Large), GUIColor(1, 0.5f, 0)]
        [HorizontalGroup("Actions")]
        private bool ValidateSetup()
        {
            if (wheelMesh == null || frameMesh == null ||
                forkMesh == null || axleMesh == null)
            {
                Debug.LogError("Missing required meshes!");
                return false;
            }

            if (wheelMaterial == null || frameMaterial == null ||
                forkMaterial == null || axleMaterial == null)
            {
                Debug.LogError("Missing required materials!");
                return false;
            }

            return true;
        }

        [Button(ButtonSizes.Large), GUIColor(1, 0, 0)]
        [HorizontalGroup("Actions")]
        public void DestroyMotorcycle()
        {
            if (bikeRoot != null)
                DestroyImmediate(bikeRoot);
        }
    }
}