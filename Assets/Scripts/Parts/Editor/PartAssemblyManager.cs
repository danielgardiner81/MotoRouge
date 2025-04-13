using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Parts.Components;
using Parts.Editor.Drawers;
using Unity.Mathematics;

namespace Parts.Editor
{
    public class PartAssemblyManager : SerializedMonoBehaviour
    {
        [TitleGroup("Parts")]
        [TableList(ShowIndexLabels = true, DrawScrollView = true), LabelText("Available Parts")]
        public List<PartDefinition> availableParts = new();

        [TitleGroup("Assembly")]
        [InlineEditor(InlineEditorObjectFieldModes.Foldout)]
        public GameObject assemblyRoot;

        [TitleGroup("Assembly")]
        [TableList(ShowIndexLabels = true, DrawScrollView = true)]
        private List<PartInstance> assembledParts = new();

        [TitleGroup("Debug")]
        [ShowInInspector, ReadOnly]
        private Dictionary<string, ConnectionPoint> activeConnections = new();

        private Dictionary<string, PartComponent> partComponents = new();

        [Button("Start New Assembly"), GUIColor(0.3f, 0.8f, 0.3f)]
        private void StartNewAssembly()
        {
            if (assemblyRoot != null)
            {
                DestroyImmediate(assemblyRoot);
            }

            assemblyRoot = new GameObject("Motorcycle Assembly");
            assembledParts.Clear();
            activeConnections.Clear();
            partComponents.Clear();
        }

        [Button("Add Part"), GUIColor(0.3f, 0.8f, 0.3f), EnableIf("@assemblyRoot != null")]
        private void AddPartToAssembly(PartDefinition part)
        {
            if (part == null || assemblyRoot == null) return;

            var partObject = new GameObject(part.name);
            partObject.transform.SetParent(assemblyRoot.transform);

            var partComponent = partObject.AddComponent<PartComponent>();
            partComponent.definition = part;

            var instance = new PartInstance
            {
                definition = part,
                position = Vector3.zero,
                rotation = Quaternion.identity
            };

            assembledParts.Add(instance);
            partComponents[part.name] = partComponent;
        }

        [Button("Connect Parts"), GUIColor(0.3f, 0.8f, 0.3f)]
        private void ConnectParts(PartComponent partA, string connectionA, PartComponent partB, string connectionB)
        {
            var pointA = GetConnectionPoint(partA, connectionA);
            var pointB = GetConnectionPoint(partB, connectionB);

            if (!ValidateConnection(pointA, pointB))
            {
                Debug.LogError("Invalid connection configuration");
                return;
            }

            // Create the physical connection
            CreatePhysicalConnection(partA, pointA, partB, pointB);

            // Register the connection
            activeConnections[connectionA] = pointB;
            activeConnections[connectionB] = pointA;

            // Update the transforms to align the connection points
            AlignParts(partA, pointA, partB, pointB);
        }

        private ConnectionPoint GetConnectionPoint(PartComponent part, string connectionId)
        {
            return part.definition.connections.Find(c => c.id == connectionId);
        }

        private bool ValidateConnection(ConnectionPoint a, ConnectionPoint b)
        {
            if (a == null || b == null) return false;
            if (activeConnections.ContainsKey(a.id) || activeConnections.ContainsKey(b.id))
            {
                Debug.LogWarning("One or both connection points are already in use");
                return false;
            }

            return ConnectionPointHelper.CanConnect(a, b);
        }

        private void CreatePhysicalConnection(PartComponent partA, ConnectionPoint pointA, 
                                            PartComponent partB, ConnectionPoint pointB)
        {
            var joint = CreateJoint(pointA.type, partA.gameObject);
            if (joint == null) return;

            joint.connectedBody = partB.GetComponent<Rigidbody>();
            
            // Set joint position to connection point in world space
            var worldPosA = partA.transform.TransformPoint(pointA.localPosition);
            joint.anchor = partA.transform.InverseTransformPoint(worldPosA);
            joint.connectedAnchor = pointB.localPosition;

            // Configure joint based on type
            ConfigureJoint(joint, pointA);
        }

        private Joint CreateJoint(ConnectionType type, GameObject obj)
        {
            switch (type)
            {
                case ConnectionType.Fixed:
                    return obj.AddComponent<FixedJoint>();
                case ConnectionType.Hinge:
                    return obj.AddComponent<HingeJoint>();
                case ConnectionType.BallSocket:
                    return obj.AddComponent<CharacterJoint>();
                default:
                    return null;
            }
        }

        private void ConfigureJoint(Joint joint, ConnectionPoint point)
        {
            switch (point.type)
            {
                case ConnectionType.Hinge when joint is HingeJoint hingeJoint:
                    hingeJoint.axis = point.hingeAxis;
                    hingeJoint.limits = new JointLimits
                    {
                        min = point.hingeLimit.x,
                        max = point.hingeLimit.y
                    };
                    hingeJoint.useLimits = true;
                    break;

                case ConnectionType.BallSocket when joint is CharacterJoint ballJoint:
                    ballJoint.swingAxis = point.direction;
                    ballJoint.twistLimitSpring = new SoftJointLimitSpring { spring = 100, damper = 10 };
                    ballJoint.lowTwistLimit = new SoftJointLimit { limit = -point.maxTwistAngle };
                    ballJoint.highTwistLimit = new SoftJointLimit { limit = point.maxTwistAngle };
                    break;
            }
        }

        private void AlignParts(PartComponent partA, ConnectionPoint pointA, 
                              PartComponent partB, ConnectionPoint pointB)
        {
            // Calculate the world position and rotation for the connection points
            var worldPosA = partA.transform.TransformPoint(pointA.localPosition);
            var worldDirA = partA.transform.TransformDirection(pointA.direction);
            
            var worldPosB = partB.transform.TransformPoint(pointB.localPosition);
            var worldDirB = partB.transform.TransformDirection(pointB.direction);

            // Calculate the rotation needed to align the connection directions
            var rotationDelta = Quaternion.FromToRotation(worldDirB, -worldDirA);
            
            // Apply the rotation to part B
            partB.transform.rotation = rotationDelta * partB.transform.rotation;

            // Move part B so the connection points align
            var offset = worldPosA - partB.transform.TransformPoint(pointB.localPosition);
            partB.transform.position += offset;
        }

        [Button("Reset Assembly"), GUIColor(1, 0.3f, 0.3f)]
        private void ResetAssembly()
        {
            if (assemblyRoot != null)
            {
                DestroyImmediate(assemblyRoot);
            }
            assembledParts.Clear();
            activeConnections.Clear();
            partComponents.Clear();
        }

        private void OnDrawGizmos()
        {
            if (!assemblyRoot) return;

            foreach (var part in partComponents.Values)
            {
                if (!part) continue;

                foreach (var connection in part.definition.connections)
                {
                    var worldPos = part.transform.TransformPoint(connection.localPosition);
                    var worldDir = part.transform.TransformDirection(connection.direction);

                    // Draw connection point
                    Gizmos.color = activeConnections.ContainsKey(connection.id) 
                        ? Color.green 
                        : ConnectionPointHelper.GetColorForType(connection.type);
                    
                    Gizmos.DrawWireSphere(worldPos, connection.radius);
                    Gizmos.DrawRay(worldPos, worldDir * connection.radius * 2);

#if UNITY_EDITOR
                    // Draw labels in scene view
                    UnityEditor.Handles.Label(worldPos + Vector3.up * connection.radius, connection.id);
#endif
                }
            }
        }
    }
}