using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;
namespace SimpleUnityPhysics
{
    public class SimplePhysics : MonoBehaviour
    {

        public float dt = 0.01f;
        public int itersPerFrame = 10;
        public int collisionIters = 3;
        public int boneIters = 5;
        public float friction = 0.99f;


        Vector3[] initialPositions;

        Bone[] bones;
        SimpleRigidbody3D[] rigidbodies;

        void Awake()
        {
            rigidbodies = FindObjectsOfType<SimpleRigidbody3D>();

            bones = FindObjectsOfType<Bone>();

            initialPositions = new Vector3[rigidbodies.Length];

            for (int i = 0; i < rigidbodies.Length; i++)
            {
                initialPositions[i] = rigidbodies[i].transform.position;
            }

            ResetSimulation();

            foreach (SimpleRigidbody3D rigidbody in rigidbodies)
            {
                rigidbody.distances = new float[rigidbodies.Length];
            }



            foreach (Bone bone in FindObjectsOfType<Bone>())
            {
                HashSet<SimpleRigidbody3D> newConnectedComponent = new HashSet<SimpleRigidbody3D>();

                newConnectedComponent.Add(bone.myRigidbody);
                newConnectedComponent.Add(bone.other);


                if (bone.myRigidbody.connectedComponent != null)
                {
                    newConnectedComponent.UnionWith(bone.myRigidbody.connectedComponent);
                }

                if (bone.other.connectedComponent != null)
                {
                    newConnectedComponent.UnionWith(bone.other.connectedComponent);
                }



                bone.myRigidbody.connectedComponent = newConnectedComponent;
                bone.other.connectedComponent = newConnectedComponent;
            }

        }

        public Vector3 gravity = new Vector3(0, -9.81f, 0);

        public static void VectorAfterNormalForce(float forceX, float forceY, float forceZ, float normalX, float normalY, float normalZ, out float resX, out float resY, out float resZ)
        {
            float rejX, rejY, rejZ;
            SimpleRigidbody3D.VectorRejection(forceX, forceY, forceZ, normalX, normalY, normalZ, out rejX, out rejY, out rejZ);

            float projX, projY, projZ;
            SimpleRigidbody3D.VectorProjection(forceX, forceY, forceZ, normalX, normalY, normalZ, out projX, out projY, out projZ);

            float dotProd = forceX * normalX + forceY * normalY + forceZ * normalZ;

            float pointingInSameDirection = dotProd / Mathf.Abs(dotProd);
            // Not pointing in same direction
            if (pointingInSameDirection <= 0.0f)
            {
                resX = rejX;
                resY = rejY;
                resZ = rejZ;
            }
            // Pointing in same direction
            else
            {
                resX = rejX + projX;
                resY = rejY + projY;
                resZ = rejZ + projZ;
            }
        }

        void DoTick()
        {
            foreach (SimpleRigidbody3D rigi in rigidbodies)
            {
                rigi.UpdateMe();
            }

            for (int i = 0; i < boneIters; i++)
            {
                foreach (Bone bone in bones)
                {
                    float prevDistance = bone.prevDistance;
                    ApplyBone(bone.myRigidbody, bone.other, bone.distance, ref prevDistance, 0.001f);
                    bone.prevDistance = prevDistance;
                }
            }
        }

        


        public void ApplyBone(SimpleRigidbody3D left, SimpleRigidbody3D right, float desiredDist, ref float prevDistance, float distThreshold)
        {
            if (left == right) { return; }

            float pdx = left.tmpX - right.tmpX;
            float pdy = left.tmpY - right.tmpY;
            float pdz = left.tmpZ - right.tmpZ;

            float distanceNow = Mathf.Sqrt(pdx * pdx + pdy * pdy + pdz * pdz);

           // float distanceNow = Vector3f.Distance(left.tmpPosition, right.tmpPosition);

            bool changing = false;
            if (Mathf.Abs(desiredDist - distanceNow) >= distThreshold)
            {
                changing = true;
            }
            else
            {
                prevDistance = desiredDist;
                return;
            }


            float npdx, npdy, npdz;

            SimpleRigidbody3D.Normalize(pdx, pdy, pdz, out npdx, out npdy, out npdz);

            float normX, normY, normZ;

            float normLen = ((distanceNow - desiredDist) / Mathf.Abs(desiredDist - distanceNow));

            normX = npdx * normLen;
            normY = npdy * normLen;
            normZ = npdz * normLen;

            if (changing)
            {
                float myNotNormX, myNotNormY, myNotNormZ;
                VectorAfterNormalForce(left.velocityX, left.velocityY, left.velocityZ, -normX, -normY, -normZ, out myNotNormX, out myNotNormY, out myNotNormZ);

                float otherNotAlongX, otherNotAlongY, otherNotAlongZ;
                VectorAfterNormalForce(right.velocityX, right.velocityY, right.velocityZ, normX, normY, normZ, out otherNotAlongX, out otherNotAlongY, out otherNotAlongZ);

                float myAlongNormX = left.velocityX - myNotNormX;
                float myAlongNormY = left.velocityY - myNotNormY;
                float myAlongNormZ = left.velocityZ - myNotNormZ;

                float otherAlongNormX = right.velocityX - otherNotAlongX;
                float otherAlongNormY = right.velocityY - otherNotAlongY;
                float otherAlongNormZ = right.velocityZ - otherNotAlongZ;

                float avgAlongNormX = (myAlongNormX + otherAlongNormX) / 2.0f;
                float avgAlongNormY = (myAlongNormY + otherAlongNormY) / 2.0f;
                float avgAlongNormZ = (myAlongNormZ + otherAlongNormZ) / 2.0f;

                float colNormMoveX = npdx;
                float colNormMoveY = npdy;
                float colNormMoveZ = npdz;


                left.velocityX = myNotNormX + avgAlongNormX;
                left.velocityY = myNotNormY + avgAlongNormY;
                left.velocityZ = myNotNormZ + avgAlongNormZ;

                right.velocityX = otherNotAlongX + avgAlongNormX;
                right.velocityY = otherNotAlongY + avgAlongNormY;
                right.velocityZ = otherNotAlongZ + avgAlongNormZ;

                if (prevDistance != desiredDist)
                {
                    right.velocityX += -colNormMoveX * (desiredDist - prevDistance);
                    right.velocityY += -colNormMoveY * (desiredDist - prevDistance);
                    right.velocityZ += -colNormMoveZ * (desiredDist - prevDistance);

                    left.velocityX += colNormMoveX * (desiredDist - prevDistance);
                    left.velocityY += colNormMoveY * (desiredDist - prevDistance);
                    left.velocityZ += colNormMoveZ * (desiredDist - prevDistance);
                }
                prevDistance = distanceNow;
            }
            else
            {
                prevDistance = desiredDist;
            }

        }

        public void ResetSimulation()
        {
            for (int i = 0; i < rigidbodies.Length; i++)
            {
                rigidbodies[i].tmpX = initialPositions[i].x;
                rigidbodies[i].tmpY = initialPositions[i].y;
                rigidbodies[i].tmpZ = initialPositions[i].z;
            }

            foreach (SimpleRigidbody3D rigid in rigidbodies)
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
                    Vector3 thingPos = new Vector3(thing.tmpX, thing.tmpY, thing.tmpZ);
                    if (thingPos != thing.transform.position)
                    {
                        Vector3 dMe = thing.transform.position - thingPos;
                        thing.velocityX += (dMe / 4.0f).x;
                        thing.velocityY += (dMe / 4.0f).y;
                        thing.velocityZ += (dMe / 4.0f).z;

                        thing.tmpX = thing.transform.position.x;
                        thing.tmpY = thing.transform.position.y;
                        thing.tmpZ = thing.transform.position.z;
                    }
                }

                DoTick();

                foreach (SimpleRigidbody3D thing in rigidbodies)
                {
                    thing.transform.position = new Vector3(thing.tmpX, thing.tmpY, thing.tmpZ);
                }
            }
            else if (simulationType == SimulationType.Visable)
            {
                DoTick();

                foreach (SimpleRigidbody3D thing in rigidbodies)
                {
                    thing.transform.position = new Vector3(thing.tmpX, thing.tmpY, thing.tmpZ);
                }
            }
            else // if(simulationType == SimulationType.Simulated)
            {
                DoTick();
            }
        }
        

        public string saveConfigName = "scene.json";
    }
}