using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SimpleUnityPhysics
{
    public class SimpleRigidbody3D : MonoBehaviour
    {


        SimplePhysics time;

        bool added = false;

        public int index;

        List<SimpleRigidbody3D> otherColliders;
        List<BoxCollider> staticColliders;

        public Vector3 initialPosition;



        public void Awake()
        {
            hitNormals = new List<Vector3>();
            initialPosition = transform.position;
            tmpPosition = transform.position;

            Collider[] otherThings = FindObjectsOfType<Collider>();
            otherColliders = new List<SimpleRigidbody3D>();
            staticColliders = new List<BoxCollider>();

            for (int i = 0; i < otherThings.Length; i++)
            {
                SimpleRigidbody3D rigid = otherThings[i].GetComponent<SimpleRigidbody3D>();
                if (rigid != null && rigid != this)
                {
                    otherColliders.Add(rigid);
                }
                else if (otherThings[i].GetComponent<BoxCollider>() != null)
                {
                    staticColliders.Add(otherThings[i].GetComponent<BoxCollider>());
                }
            }


            time = FindObjectOfType<SimplePhysics>();
            radius = transform.localScale.x / 2.0f;

        }


        public float[] distances;
        
        public void Start()
        {
            if (!added) { time.AddMeToTickHandler(this, UpdateMe); added = true; }
        }

        public HashSet<SimpleRigidbody3D> connectedComponent;

        public Vector3 tmpPosition;


        public Vector3 velocity;
        [HideInInspector]
        public float radius = 1.0f;

        public List<Vector3> intersectionNormals = new List<Vector3>();
        // See https://en.wikipedia.org/wiki/Vector_projection
        public static Vector3 VectorProjection(Vector3 a, Vector3 b)
        {
            return b * Vector3.Dot(a, b.normalized);
        }

        public static Vector3 VectorRejection(Vector3 a, Vector3 b)
        {
            return a - VectorProjection(a, b);
        }

        public int collisionIters = 10;

        // Accessing zero is slow for some reason?
        public Vector3 zeroThing = new Vector3(0, 0, 0);

        public bool SphereSphereCollision(SimpleRigidbody3D me, SimpleRigidbody3D other, out Vector3 point, out Vector3 normal)
        {
            // Accessing properties of compoments are slow
            float myRadius = me.radius;
            float otherRadius = other.radius;
            // I need to do this because for some reason unity Vector3.Distance is slow
            float dx = me.tmpPosition.x - other.tmpPosition.x;
            float dy = me.tmpPosition.y - other.tmpPosition.y;
            float dz = me.tmpPosition.z - other.tmpPosition.z;
            float distance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
            if (distance <= myRadius + otherRadius)
            {
                if (distance == 0.0f)
                {
                    // If they are equal, do a pertubation
                    // This is typically random, but I don't have it be random so the results are deterministic
                    point = me.tmpPosition + Vector3.one.normalized / me.radius / 10.0f;
                    normal = (me.tmpPosition - point).normalized;
                }
                else
                {
                    point = (me.tmpPosition * myRadius + other.tmpPosition * otherRadius) / (myRadius + otherRadius);
                    normal = (me.tmpPosition - point).normalized;
                }
                return true;
            }
            else
            {
                point = zeroThing;
                normal = zeroThing;
                return false;
            }
        }

        public bool SphereBoxCollision(SimpleRigidbody3D me, BoxCollider other, out Vector3 point, out Vector3 normal)
        {
            Vector3 closePoint = other.ClosestPointOnBounds(me.tmpPosition);
            

            float distance = Vector3.Distance(me.tmpPosition, closePoint);
            if (distance <= me.radius)
            {
                if (distance == 0.0f)
                {
                    point = me.tmpPosition + Vector3.one.normalized / me.radius / 10.0f;
                    normal = (me.tmpPosition - point).normalized;
                }
                else
                {
                    point = closePoint;
                    normal = (me.tmpPosition - closePoint).normalized;
                }
                return true;
            }
            else
            {
                point = zeroThing;
                normal = zeroThing;
                return false;
            }
        }

        public List<SimpleRigidbody3D> connectedComponentVisi;

        public List<Vector3> hitNormals = new List<Vector3>();

        public bool FixCollisions()
        {
            bool fixedSomething = false;
            foreach (SimpleRigidbody3D other in otherColliders)
            {
                Vector3 collisionPoint;
                Vector3 collisionNormal;
                if (SphereSphereCollision(this, other, out collisionPoint, out collisionNormal))
                {
                    fixedSomething = true;
                    Vector3 newVelocity = SimplePhysics.VectorAfterNormalForce(velocity, collisionNormal);
                    Vector3 lostVelocity = velocity - newVelocity;
                    Vector3 otherNewVelocity = SimplePhysics.VectorAfterNormalForce(other.velocity, -collisionNormal);
                    Vector3 otherLostVelocity = other.velocity - otherNewVelocity;

                    Vector3 averageLostVelocity = (lostVelocity + otherLostVelocity) / 2.0f;

                    velocity = newVelocity * time.friction + averageLostVelocity;

                    
                    other.velocity = otherNewVelocity * time.friction + averageLostVelocity;
                    

                    Vector3 moveOffset = collisionNormal * (radius - Vector3.Distance(collisionPoint, tmpPosition));
                    tmpPosition += moveOffset;
                    other.tmpPosition -= moveOffset;
                }
            }


            foreach (BoxCollider other in staticColliders)
            {
                Vector3 collisionPoint;
                Vector3 collisionNormal;
                if (SphereBoxCollision(this, other, out collisionPoint, out collisionNormal))
                {
                    fixedSomething = true;
                    Vector3 newVelocity = SimplePhysics.VectorAfterNormalForce(velocity, collisionNormal);


                    velocity = newVelocity * time.friction;
                    
                    Vector3 moveOffset = collisionNormal * (radius - Vector3.Distance(collisionPoint, tmpPosition));
                    tmpPosition += moveOffset;
                }
            }

            return fixedSomething;
        }

        public int numDesiredPositions = 0;
        public Vector3 desiredPosition;


        public void ResetMe()
        {
            velocity = Vector3.zero;
            curForce = Vector3.zero;
            intersectionNormals = new List<Vector3>();
            hitNormals = new List<Vector3>();
        }


        Collider[] colliders;

        public Vector3 curForce;
        public Vector3 newForce;
        
        public void UpdateMe()
        {
            for (int i = 0; i < time.itersPerFrame; i++)
            {
                float displacement = time.dt / time.itersPerFrame;

                velocity += curForce * displacement;
                tmpPosition += velocity * displacement;

                FixCollisions();
            }

        }

        [HideInInspector]
        public int id;
    }
}