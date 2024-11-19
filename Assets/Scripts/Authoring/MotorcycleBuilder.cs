using UnityEngine;
using Sirenix.OdinInspector;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Mathematics;
using Material = UnityEngine.Material;

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

    [FoldoutGroup("Dimensions")]
    [Header("Wheel Settings"), MinValue(0.1f)]
    public float wheelRadius = 0.4f;

    [FoldoutGroup("Dimensions"), MinValue(0.01f)]
    public float wheelThickness = 0.1f;

    [FoldoutGroup("Dimensions")]
    [Header("Frame Settings"), MinValue(0.1f)]
    public float frameHeight = 0.8f;

    [FoldoutGroup("Dimensions"), MinValue(0.1f)]
    public float frameLength = 1.6f;

    [FoldoutGroup("Dimensions")]
    [Header("Fork Settings"), MinValue(0.1f)]
    public float forkLength = 0.6f;

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

    private GameObject BuildFrame()
    {
        var frame = new GameObject("Frame");
        frame.transform.SetParent(bikeRoot.transform);

        // Create frame mesh
        var frameObj = CreateMeshObject("FrameMesh", frameMesh, frameMaterial);
        frameObj.transform.SetParent(frame.transform);
        frameObj.transform.localPosition = new Vector3(0, frameHeight * 0.5f, 0);

        // Add physics body
        var physicsBody = frame.AddComponent<PhysicsBodyAuthoring>();
        physicsBody.Mass = frameMass;

        // Add collider
        var collider = frame.AddComponent<PhysicsShapeAuthoring>();
        var boxGeometry = new BoxGeometry()
        {
            Size = new float3(0.3f, frameHeight, frameLength),
            Orientation = quaternion.identity,
            Center = new float3(0, frameHeight * 0.5f, 0)
        };
        collider.SetBox(boxGeometry);

        return frame;
    }

    private GameObject BuildWheel(string name, bool isFront)
    {
        var wheel = new GameObject(name);
        wheel.transform.SetParent(bikeRoot.transform);

        // Create wheel mesh
        var wheelObj = CreateMeshObject($"{name}Mesh", wheelMesh, wheelMaterial);
        wheelObj.transform.SetParent(wheel.transform);
        wheelObj.transform.localScale = new Vector3(wheelRadius * 2, wheelThickness, wheelRadius * 2);

        // Position wheel
        float zPos = isFront ? frameLength * 0.5f : -frameLength * 0.5f;
        wheel.transform.localPosition = new Vector3(0, wheelRadius, zPos);

        // Add physics
        var physicsBody = wheel.AddComponent<PhysicsBodyAuthoring>();
        physicsBody.Mass = wheelMass;

        var collider = wheel.AddComponent<PhysicsShapeAuthoring>();
        var cylinderGeometry = new CylinderGeometry()
        {
            Height = wheelThickness,
            Radius = wheelRadius,
            Orientation = quaternion.Euler(0, 0, 90),
            Center = float3.zero
        };
        collider.SetCylinder(cylinderGeometry);

        return wheel;
    }

    private GameObject BuildForks(string prefix, bool isFront)
    {
        var forkRoot = new GameObject($"{prefix}Forks");
        forkRoot.transform.SetParent(bikeRoot.transform);

        float zOffset = isFront ? frameLength * 0.5f : -frameLength * 0.5f;
        float angle = isFront ? -30 : 30;

        // Left fork
        var leftFork = CreateMeshObject($"{prefix}ForkLeft", forkMesh, forkMaterial);
        leftFork.transform.SetParent(forkRoot.transform);
        leftFork.transform.localPosition = new Vector3(-wheelThickness * 2, frameHeight * 0.5f, zOffset);
        leftFork.transform.localRotation = Quaternion.Euler(angle, 0, 0);

        // Right fork
        var rightFork = CreateMeshObject($"{prefix}ForkRight", forkMesh, forkMaterial);
        rightFork.transform.SetParent(forkRoot.transform);
        rightFork.transform.localPosition = new Vector3(wheelThickness * 2, frameHeight * 0.5f, zOffset);
        rightFork.transform.localRotation = Quaternion.Euler(angle, 0, 0);

        // Axle
        var axle = CreateMeshObject($"{prefix}Axle", axleMesh, axleMaterial);
        axle.transform.SetParent(forkRoot.transform);
        axle.transform.localPosition = new Vector3(0, wheelRadius, zOffset);
        axle.transform.localRotation = Quaternion.Euler(0, 0, 90);

        return forkRoot;
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