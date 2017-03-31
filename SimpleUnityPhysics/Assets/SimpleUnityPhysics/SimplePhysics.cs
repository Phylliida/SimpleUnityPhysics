using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
namespace SimpleUnityPhysics
{
    public class SimplePhysics : MonoBehaviour
    {

        public float dt = 0.1f;
        public int itersPerFrame = 10;
        public int collisionIters = 3;
        public int jointIters = 5;
        public float friction = 0.99f;

        public bool watchGo = false;

        public void AddMeToTickHandler(Component thingToAdd, OnTickHandler UpdateFunction)
        {
            string uniqueIdentifier = "" + thingToAdd.GetInstanceID();
            ordering.Add(new KeyValuePair<string, SimplePhysics.OnTickHandler>(uniqueIdentifier, UpdateFunction));
            ordering.Sort((a, b) => a.Key.CompareTo(b.Key));
        }

        public HashSet<HashSet<SimpleRigidbody3D>> connectedComponents;

        void Awake()
        {
            randomGen = new System.Random(27);
            UnityEngine.Random.InitState(28);
            ordering = new List<KeyValuePair<string, OnTickHandler>>();
            connectedComponents = new HashSet<HashSet<SimpleRigidbody3D>>();
        }


        public Vector3[] initialPositions;
        public Quaternion[] initialRotations;


        SimpleJoint[] joints;
        Bone[] bones;
        SimpleRigidbody3D[] actualObjects;


        public System.Random randomGen;

        // Use this for initialization
        void Start()
        {
            actualObjects = FindObjectsOfType<SimpleRigidbody3D>();

            bones = FindObjectsOfType<Bone>();
            joints = FindObjectsOfType<SimpleJoint>();

            initialPositions = new Vector3[actualObjects.Length];
            initialRotations = new Quaternion[actualObjects.Length];

            for (int i = 0; i < actualObjects.Length; i++)
            {
                initialPositions[i] = actualObjects[i].transform.position;
                initialRotations[i] = actualObjects[i].transform.rotation;
            }

            ResetSimulation();

            rigidbodies = actualObjects;


            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodies[i].index = i;
            }

            foreach (SimpleRigidbody3D rigidbody in rigidbodies)
            {
                rigidbody.distances = new float[rigidbodies.Length];
                foreach (SimpleRigidbody3D other in rigidbodies)
                {
                    rigidbody.distances[other.index] = Vector3.Distance(rigidbody.initialPosition, other.initialPosition);
                }
            }



            foreach (Joint3D joint in FindObjectsOfType<Joint3D>())
            {
                if (joint.jointType == JointType.Fixed)
                {
                    HashSet<SimpleRigidbody3D> newConnectedComponent = new HashSet<SimpleRigidbody3D>();

                    newConnectedComponent.Add(joint.me);
                    newConnectedComponent.Add(joint.left);
                    newConnectedComponent.Add(joint.right);


                    if (joint.me.connectedComponent != null)
                    {
                        newConnectedComponent.UnionWith(joint.me.connectedComponent);
                    }

                    if (joint.left.connectedComponent != null)
                    {
                        newConnectedComponent.UnionWith(joint.left.connectedComponent);
                    }

                    if (joint.right.connectedComponent != null)
                    {
                        newConnectedComponent.UnionWith(joint.right.connectedComponent);
                    }


                    joint.me.connectedComponent = newConnectedComponent;
                    joint.left.connectedComponent = newConnectedComponent;
                    joint.right.connectedComponent = newConnectedComponent;
                }
            }

            foreach (Joint3D joint in FindObjectsOfType<Joint3D>())
            {
                if (joint.jointType == JointType.Free)
                {
                    if (joint.left.connectedComponent != null)
                    {
                        joint.left.connectedComponent.Add(joint.me);
                    }
                    else
                    {
                        HashSet<SimpleRigidbody3D> newConnectedComponent = new HashSet<SimpleRigidbody3D>();
                        newConnectedComponent.Add(joint.me);
                        newConnectedComponent.Add(joint.left);
                        joint.left.connectedComponent = newConnectedComponent;
                    }
                    if (joint.right.connectedComponent != null)
                    {
                        joint.right.connectedComponent.Add(joint.me);
                    }
                    else
                    {
                        HashSet<SimpleRigidbody3D> newConnectedComponent = new HashSet<SimpleRigidbody3D>();
                        newConnectedComponent.Add(joint.me);
                        newConnectedComponent.Add(joint.right);
                        joint.right.connectedComponent = newConnectedComponent;
                    }
                }
            }

            foreach (SimpleRigidbody3D rigidbody in rigidbodies)
            {
                if (rigidbody.connectedComponent != null)
                {
                    connectedComponents.Add(rigidbody.connectedComponent);
                    rigidbody.connectedComponentVisi = new List<SimpleRigidbody3D>(rigidbody.connectedComponent);
                }
            }

        }

        public List<KeyValuePair<string, OnTickHandler>> ordering;

        public delegate void OnTickHandler();


        public Vector3 gravity = new Vector3(0, -9.81f, 0);

        public static Vector3 VectorAfterNormalForce(Vector3 force, Vector3 normal)
        {
            Vector3 rejection = SimpleRigidbody3D.VectorRejection(force, normal);
            Vector3 projection = SimpleRigidbody3D.VectorProjection(force, normal);
            float pointingInSameDirection = Vector3.Dot(normal, force) / Mathf.Abs(Vector3.Dot(normal, force));
            // Not pointing in same direction
            if (pointingInSameDirection <= 0.0f)
            {
                return rejection;
            }
            // Pointing in same direction
            else
            {
                return rejection + projection;
            }
        }

        void DoTick()
        {
            foreach (SimpleRigidbody3D rigi in rigidbodies)
            {
                rigi.curForce += gravity*10.0f;
            }

            foreach (SimpleRigidbody3D rigi in rigidbodies)
            {
                rigi.UpdateMe();
            }

            foreach (SimpleRigidbody3D rigi in rigidbodies)
            {
                rigi.curForce = Vector3.zero;
            }

            foreach (SimpleJoint joint in joints)
            {
                for (int i = 0; i < itersPerFrame; i++)
                {
                    float displacement = dt / itersPerFrame;

                    joint.helperPos += joint.helperVelocity * displacement;
                }

                joint.helper.transform.position = joint.helperPos;
            }


            for (int i = 0; i < jointIters; i++)
            {
                foreach (SimpleJoint joint in joints)
                {
                    if (joint.left == joint.right || joint.right == joint.myRigidbody || joint.myRigidbody == joint.left) { continue; }

                    Vector3 distDir = ((joint.left.tmpPosition - joint.myRigidbody.tmpPosition).normalized * joint.leftBone.distance + (joint.myRigidbody.tmpPosition - joint.right.tmpPosition).normalized * joint.rightBone.distance + (joint.left.tmpPosition - joint.right.tmpPosition).normalized * (joint.leftBone.distance + joint.rightBone.distance)) / (joint.leftBone.distance * 2.0f + joint.rightBone.distance * 2.0f);
                    distDir = ((joint.left.tmpPosition - joint.myRigidbody.tmpPosition) + (joint.myRigidbody.tmpPosition - joint.right.tmpPosition) + (joint.left.tmpPosition - joint.right.tmpPosition)) / 3.0f;

                    Vector3 a, b, c;
                    GetDisplacements(joint.left.tmpPosition, joint.myRigidbody.tmpPosition, joint.right.tmpPosition, distDir.normalized * joint.leftBone.distance, distDir.normalized * joint.rightBone.distance, out a, out b, out c);


                    joint.left.velocity += a / dt;
                    joint.myRigidbody.velocity += b / dt;
                    joint.right.velocity += c / dt;



                    ApplyBone(joint.left, joint.right, joint.initialDistEndpoints, ref joint.prevDistEndpoints, 0.01f);

                }

                foreach (Bone bone in bones)
                {
                    float prevDistance = bone.prevDistance;
                    ApplyBone(bone.myRigidbody, bone.other, bone.distance, ref prevDistance, 0.001f);
                    bone.prevDistance = prevDistance;
                }
            }

        }
        public void ApplyBone(ref Vector3 leftPos, ref Vector3 leftVel, ref Vector3 rightPos, ref Vector3 rightVel, float desiredDist, ref float prevDistance, float distThreshold)
        {
            if (leftPos == rightPos) { return; }

            float distanceNow = Vector3.Distance(leftPos, rightPos);

            if (Mathf.Abs(desiredDist - distanceNow) >= distThreshold)
            {
                Vector3 collisionNormal = ((distanceNow - desiredDist) / Mathf.Abs(desiredDist - distanceNow)) * (leftPos - rightPos).normalized;

                Vector3 myNotAlongNormal = VectorAfterNormalForce(leftVel, -collisionNormal);
                Vector3 otherNotAlongNormal = VectorAfterNormalForce(rightVel, collisionNormal);

                Vector3 myAlongNormal = leftVel - myNotAlongNormal;
                Vector3 otherAlongNormal = rightVel - otherNotAlongNormal;



                Vector3 averageAlongNormal = (myAlongNormal + otherAlongNormal) / 2.0f;


                Vector3 collisionNormalMove = -(rightPos - leftPos).normalized;

                leftVel = myNotAlongNormal + averageAlongNormal;
                rightVel = otherNotAlongNormal + averageAlongNormal;


                if (prevDistance != desiredDist)
                {
                    rightVel += -collisionNormalMove * (desiredDist - prevDistance);
                    leftVel += collisionNormalMove * (desiredDist - prevDistance);
                }
                prevDistance = distanceNow;
            }
            else
            {
                prevDistance = desiredDist;
            }
        }

        public void ApplyBone(SimpleRigidbody3D left, SimpleRigidbody3D right, float desiredDist, ref float prevDistance, float distThreshold)
        {
            if (left == right) { return; }

            float distanceNow = Vector3.Distance(left.tmpPosition, right.tmpPosition);

            bool changing = false;
            if (Mathf.Abs(desiredDist - distanceNow) >= distThreshold)
            {
                changing = true;
            }

            Vector3 collisionNormal = ((distanceNow - desiredDist) / Mathf.Abs(desiredDist - distanceNow)) * (left.tmpPosition - right.tmpPosition).normalized;

            if (changing)
            { 

                Vector3 myNotAlongNormal = VectorAfterNormalForce(left.velocity, -collisionNormal);
                Vector3 otherNotAlongNormal = VectorAfterNormalForce(right.velocity, collisionNormal);

                Vector3 myAlongNormal = left.velocity - myNotAlongNormal;
                Vector3 otherAlongNormal = right.velocity - otherNotAlongNormal;



                Vector3 averageAlongNormal = (myAlongNormal + otherAlongNormal) / 2.0f;


                Vector3 collisionNormalMove = -(right.tmpPosition - left.tmpPosition).normalized;

                left.velocity = myNotAlongNormal + averageAlongNormal;
                right.velocity = otherNotAlongNormal + averageAlongNormal;


                if (prevDistance != desiredDist)
                {
                    right.velocity += -collisionNormalMove * (desiredDist - prevDistance);
                    left.velocity += collisionNormalMove * (desiredDist - prevDistance);
                }
                prevDistance = distanceNow;
            }
            else
            {
                prevDistance = desiredDist;
            }
            
        }



        SimpleRigidbody3D[] rigidbodies;


        public float curTime = 0.0f;

        // Update is called once per frame  

        public void ResetSimulation()
        {
            for (int i = 0; i < actualObjects.Length; i++)
            {
                actualObjects[i].tmpPosition = initialPositions[i];
            }

            foreach (SimpleJoint joint in joints)
            {
                joint.helperVelocity = Vector3.zero;
            }


            foreach (SimpleRigidbody3D rigid in actualObjects)
            {
                rigid.ResetMe();
            }

            foreach (Bone bone in bones)
            {
                bone.Start();
            }
        }

        public enum SimulationType
        {
            Interactable,
            Visable,
            Simulated
        }

        public void StepSimulation(SimulationType simulationType)
        {
            if (simulationType == SimulationType.Interactable)
            {
                foreach (SimpleRigidbody3D thing in rigidbodies)
                {
                    thing.transform.GetComponent<Collider>().enabled = false;
                    if (thing.tmpPosition != thing.transform.position)
                    {
                        Vector3 dMe = thing.transform.position - thing.tmpPosition;
                        thing.curForce = Vector3.zero;
                        thing.velocity += dMe / 4.0f;
                        thing.tmpPosition = thing.transform.position;
                    }
                }

                DoTick();

                foreach (SimpleRigidbody3D thing in rigidbodies)
                {
                    thing.transform.position = thing.tmpPosition;
                }
            }
            else if (simulationType == SimulationType.Visable)
            {
                DoTick();

                foreach (SimpleRigidbody3D thing in rigidbodies)
                {
                    thing.transform.position = thing.tmpPosition;
                }
            }
            else // if(simulationType == SimulationType.Simulated)
            {
                DoTick();
            }
        }


        void Update()
        {

        }

        public void GetDisplacements(Vector3 x1, Vector3 x2, Vector3 x3, Vector3 d1, Vector3 d2, out Vector3 a, out Vector3 b, out Vector3 c)
        {
            // a + b + c = 0
            // x1 + a - x2 - b = d1
            // x2 + b - x3 - c = d2
            Vector3 w2 = d1 - x1 + x2;
            Vector3 w3 = d2 - x2 + x3;

            a = 2.0f * w2 / 3.0f + w3 / 3.0f;
            b = -w2 / 3.0f + w3 / 3.0f;
            c = -w2 / 3.0f - 2.0f * w3 / 3.0f;
        }

        public string saveConfigName = "scene.json";
    }
}