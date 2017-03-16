using UnityEngine;
using System.Collections;
namespace SimpleUnityPhysics
{
    public class SimpleRigidbody2D : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            radius = GetComponent<CircleCollider2D>().radius;
        }

        public Vector2 position { get { return transform.position; } set { transform.position = value; } }

        public float gravity = 9.81f;
        public int iters = 10;
        public float rootDist = 0.001f;
        public Vector3 velocity;
        public float angularVelocity;
        [HideInInspector]
        public float radius = 1.0f;

        // See https://en.wikipedia.org/wiki/Vector_projection
        public Vector3 VectorProjection(Vector3 a, Vector3 b)
        {
            return b * Vector3.Dot(a, b.normalized);
        }

        public Vector3 VectorRejection(Vector3 a, Vector3 b)
        {
            return a - VectorProjection(a, b);
        }

        public int collisionIters = 10;

        public void FixCollisions()
        {
            transform.GetComponent<Collider2D>().enabled = false;
            float displacement = rootDist / iters;

            RaycastHit2D[] collisions = Physics2D.CircleCastAll(transform.position - velocity.normalized * displacement, radius, velocity.normalized, displacement);


            for (int i = 0; i < collisionIters; i++)
            {
                if (collisions.Length != 0)
                {
                    float closestDist = float.MaxValue;
                    RaycastHit2D closest = collisions[0];
                    foreach (RaycastHit2D collision in collisions)
                    {
                        if (collision.distance < closestDist)
                        {
                            closestDist = collision.distance;
                            closest = collision;
                        }
                    }

                    //transform.position = (-new Vector3(collision.point.x, collision.point.y) + transform.position).normalized*displacement;
                    // Get remaining velocity that is not in direction of normal
                    velocity = VectorRejection(velocity, closest.normal);
                    Vector3 moveOffset = new Vector3(closest.normal.x, closest.normal.y).normalized * (radius - Vector2.Distance(closest.point, transform.position));

                    if (closest.collider.GetComponent<SimpleRigidbody2D>())
                    {
                        transform.position += moveOffset / 2.0f;
                        closest.transform.position -= moveOffset / 2.0f;
                    }
                    else
                    {
                        transform.position += moveOffset;
                    }


                }
                else
                {
                }
            }
            transform.GetComponent<Collider2D>().enabled = true;
        }


        // Update is called once per frame
        void FixedUpdate()
        {
            velocity = new Vector3(velocity.x, velocity.y - gravity * rootDist, 0);

            for (int i = 0; i < iters; i++)
            {
                FixCollisions();
                float displacement = rootDist / iters;
                transform.position += velocity * displacement;
                transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + angularVelocity * displacement);
            }
        }
    }
}