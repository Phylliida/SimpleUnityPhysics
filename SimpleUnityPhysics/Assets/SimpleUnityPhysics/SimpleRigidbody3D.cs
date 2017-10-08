using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SimpleUnityPhysics
{
    public class SimpleRigidbody3D : MonoBehaviour
    {
        [HideInInspector]
        SimplePhysics time;
        [HideInInspector]
        List<SimpleRigidbody3D> otherColliders;
        [HideInInspector]
        List<BoxCollider> staticColliders;
        [HideInInspector]
        public Vector3 initialPosition;
        [HideInInspector]
        public float tmpX, tmpY, tmpZ;


        public void Awake()
        {
            initialPosition = transform.position;
            tmpX = transform.position.x;
            tmpY = transform.position.y;
            tmpZ = transform.position.z;

            Collider[] otherThings = FindObjectsOfType<Collider>();
            otherColliders = new List<SimpleRigidbody3D>();
            staticColliders = new List<BoxCollider>();

            foreach (SimpleRigidbody3D rigi in FindObjectsOfType<SimpleRigidbody3D>())
            {
                if (rigi == this)
                {
                    continue;
                }
                otherColliders.Add(rigi);
            }

            for (int i = 0; i < otherThings.Length; i++)
            {
                if (otherThings[i].GetComponent<BoxCollider>() != null)
                {
                    staticColliders.Add(otherThings[i].GetComponent<BoxCollider>());
                }
            }


            time = FindObjectOfType<SimplePhysics>();
            radius = transform.localScale.x / 2.0f;

        }

        [HideInInspector]
        public float[] distances;

        [HideInInspector]
        public HashSet<SimpleRigidbody3D> connectedComponent;

        [HideInInspector]
        public float velocityX, velocityY, velocityZ;

        [HideInInspector]
        public float radius = 1.0f;

        // See https://en.wikipedia.org/wiki/Vector_projection

        public static void Normalize(float x, float y, float z, out float nx, out float ny, out float nz)
        {
            float mag = x * x + y * y + z * z;
            if (mag == 0)
            {
                nx = x;
                ny = y;
                nz = z;
            }
            else
            {
                mag = Mathf.Sqrt(mag);
                nx = x / mag;
                ny = y / mag;
                nz = z / mag;
            }
        }

        public static void VectorProjection(float ax, float ay, float az, float bx, float by, float bz, out float x, out float y, out float z)
        {
            float nx, ny, nz;
            Normalize(bx, by, bz, out nx, out ny, out nz);
            float dot = ax * nx + ay * ny + az * nz;
            x = bx * dot;
            y = by * dot;
            z = bz * dot;
        }

        public static void VectorRejection(float ax, float ay, float az, float bx, float by, float bz, out float x, out float y, out float z)
        {
            float nx, ny, nz;
            Normalize(bx, by, bz, out nx, out ny, out nz);
            float dot = ax * nx + ay * ny + az * nz;
            x = ax - bx * dot;
            y = ay - by * dot;
            z = az - bz * dot;
        }

        // Accessing zero is slow for some reason?

        public bool SphereSphereCollision(SimpleRigidbody3D me, SimpleRigidbody3D other, out float pointX, out float pointY, out float pointZ, out float normalX, out float normalY, out float normalZ)
        {
            // Accessing properties of compoments are slow
            float myRadius = me.radius;
            float otherRadius = other.radius;
            // I need to do this because for some reason unity Vector3f.Distance is slow
            float dx = me.tmpX - other.tmpX;
            float dy = me.tmpY - other.tmpY;
            float dz = me.tmpZ - other.tmpZ;
            float distance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

            float nx, ny, nz;
            Normalize(1, 1, 1, out nx, out ny, out nz);

            if (distance <= myRadius + otherRadius)
            {
                if (distance == 0.0f)
                {
                    pointX = me.tmpX + nx / me.radius / 10.0f;
                    pointY = me.tmpY + ny / me.radius / 10.0f;
                    pointZ = me.tmpZ + nz / me.radius / 10.0f;

                    float tmpNormalX = me.tmpX - pointX;
                    float tmpNormalY = me.tmpY - pointY;
                    float tmpNormalZ = me.tmpZ - pointZ;

                    Normalize(tmpNormalX, tmpNormalY, tmpNormalZ, out normalX, out normalY, out normalZ);
                }
                else
                {
                    pointX = (me.tmpX * myRadius + other.tmpX * otherRadius) / (myRadius + otherRadius);
                    pointY = (me.tmpY * myRadius + other.tmpY * otherRadius) / (myRadius + otherRadius);
                    pointZ = (me.tmpZ * myRadius + other.tmpZ * otherRadius) / (myRadius + otherRadius);

                    float tmpNormalX = me.tmpX - pointX;
                    float tmpNormalY = me.tmpY - pointY;
                    float tmpNormalZ = me.tmpZ - pointZ;

                    Normalize(tmpNormalX, tmpNormalY, tmpNormalZ, out normalX, out normalY, out normalZ);
                }
                return true;
            }
            else
            {
                pointX = 0; pointY = 0; pointZ = 0;
                normalX = 0; normalY = 0; normalZ = 0;
                return false;
            }
        }

        public bool SphereBoxCollision(SimpleRigidbody3D me, BoxCollider other, out float pointX, out float pointY, out float pointZ, out float normalX, out float normalY, out float normalZ)
        {
            Vector3 closePoint = other.ClosestPointOnBounds(new Vector3(me.tmpX, me.tmpY, me.tmpZ));

            float dx = me.tmpX - closePoint.x;
            float dy = me.tmpY - closePoint.y;
            float dz = me.tmpZ - closePoint.z;
            float distance = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
            if (distance <= me.radius)
            {

                float nx, ny, nz;
                Normalize(1, 1, 1, out nx, out ny, out nz);

                if (distance == 0.0f)
                {

                    pointX = me.tmpX + nx / me.radius / 10.0f;
                    pointY = me.tmpY + ny / me.radius / 10.0f;
                    pointZ = me.tmpZ + nz / me.radius / 10.0f;

                    float tmpNormalX = me.tmpX - pointX;
                    float tmpNormalY = me.tmpY - pointY;
                    float tmpNormalZ = me.tmpZ - pointZ;

                    Normalize(tmpNormalX, tmpNormalY, tmpNormalZ, out normalX, out normalY, out normalZ);
                }
                else
                {
                    pointX = closePoint.x;
                    pointY = closePoint.y;
                    pointZ = closePoint.z;

                    float tmpNormalX = me.tmpX - pointX;
                    float tmpNormalY = me.tmpY - pointY;
                    float tmpNormalZ = me.tmpZ - pointZ;

                    Normalize(tmpNormalX, tmpNormalY, tmpNormalZ, out normalX, out normalY, out normalZ);
                }
                return true;
            }
            else
            {
                pointX = 0; pointY = 0; pointZ = 0;
                normalX = 0; normalY = 0; normalZ = 0;
                return false;
            }
        }

        public bool FixCollisions()
        {
            bool fixedSomething = false;
            for (int i = 0; i < otherColliders.Count; i++)
            {
                SimpleRigidbody3D other = otherColliders[i];
                float colX, colY, colZ;
                float normX, normY, normZ;
                
                if (SphereSphereCollision(this, other, out colX, out colY, out colZ, out normX, out normY, out normZ))
                {
                    fixedSomething = true;

                    float newVelX, newVelY, newVelZ;
                    SimplePhysics.VectorAfterNormalForce(velocityX, velocityY, velocityZ, normX, normY, normZ, out newVelX, out newVelY, out newVelZ);
                    float lostVelX = velocityX - newVelX;
                    float lostVelY = velocityY - newVelY;
                    float lostVelZ = velocityZ - newVelZ;

                    float otherNewVelX, otherNewVelY, otherNewVelZ;
                    SimplePhysics.VectorAfterNormalForce(other.velocityX, other.velocityY, other.velocityZ, -normX, -normY, -normZ, out otherNewVelX, out otherNewVelY, out otherNewVelZ);
                    float otherLostVelX = other.velocityX - otherNewVelX;
                    float otherLostVelY = other.velocityY - otherNewVelY;
                    float otherLostVelZ = other.velocityZ - otherNewVelZ;

                    float avgLostVelX = (lostVelX + otherLostVelX) / 2.0f;
                    float avgLostVelY = (lostVelY + otherLostVelY) / 2.0f;
                    float avgLostVelZ = (lostVelZ + otherLostVelZ) / 2.0f;

                    velocityX = newVelX * time.friction + avgLostVelX;
                    velocityY = newVelY * time.friction + avgLostVelY;
                    velocityZ = newVelZ * time.friction + avgLostVelZ;

                    other.velocityX = otherNewVelX * time.friction + avgLostVelX;
                    other.velocityY = otherNewVelY * time.friction + avgLostVelY;
                    other.velocityZ = otherNewVelZ * time.friction + avgLostVelZ;

                    float dx = colX - tmpX;
                    float dy = colY - tmpY;
                    float dz = colZ - tmpZ;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);


                    float moveOffsetX = normX * (radius - dist);
                    float moveOffsetY = normY * (radius - dist);
                    float moveOffsetZ = normZ * (radius - dist);

                    tmpX += moveOffsetX;
                    tmpY += moveOffsetY;
                    tmpZ += moveOffsetZ;

                    other.tmpX -= moveOffsetX;
                    other.tmpY -= moveOffsetY;
                    other.tmpZ -= moveOffsetZ;
                }
            }

            for (int i = 0; i < staticColliders.Count; i++)
            {
                BoxCollider other = staticColliders[i];
                float colX, colY, colZ;
                float normX, normY, normZ;

                if (SphereBoxCollision(this, other, out colX, out colY, out colZ, out normX, out normY, out normZ))
                {
                    fixedSomething = true;

                    float newVelX, newVelY, newVelZ;
                    SimplePhysics.VectorAfterNormalForce(velocityX, velocityY, velocityZ, normX, normY, normZ, out newVelX, out newVelY, out newVelZ);
                    velocityX = newVelX * time.friction;
                    velocityY = newVelY * time.friction;
                    velocityZ = newVelZ * time.friction;


                    float dx = colX - tmpX;
                    float dy = colY - tmpY;
                    float dz = colZ - tmpZ;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy + dz * dz);

                    float moveOffsetX = normX * (radius - dist);
                    float moveOffsetY = normY * (radius - dist);
                    float moveOffsetZ = normZ * (radius - dist);

                    tmpX += moveOffsetX;
                    tmpY += moveOffsetY;
                    tmpZ += moveOffsetZ;
                }
            }

            return fixedSomething;
        }

        public void ResetMe()
        {
            velocityX = velocityY = velocityZ = 0;
        }
        
        public void UpdateMe()
        {
            for (int i = 0; i < time.itersPerFrame; i++)
            {
                float dt = time.dt / time.itersPerFrame;

                float accelerationX = time.gravity.x;
                float accelerationY = time.gravity.y;
                float accelerationZ = time.gravity.z;


                tmpX += velocityX * dt + 0.5f * accelerationX * dt * dt;
                tmpY += velocityY * dt + 0.5f * accelerationY * dt * dt;
                tmpZ += velocityZ * dt + 0.5f * accelerationZ * dt * dt;

                velocityX += accelerationX * dt;
                velocityY += accelerationY * dt;
                velocityZ += accelerationZ * dt;

                FixCollisions();
            }

        }
    }
}