using UnityEngine;
using System.Collections;
namespace SimpleUnityPhysics
{
    public class Distance2D : MonoBehaviour
    {



        public SimpleRigidbody2D other;
        SimpleRigidbody2D myRigidbody;
        public bool autoCalculateRadius = true;
        public float radius = 1.0f;
        public float moveDist = 1f;
        
        void Start()
        {
            myRigidbody = GetComponent<SimpleRigidbody2D>();
            if (autoCalculateRadius)
            {
                radius = Vector3.Distance(myRigidbody.position, other.position);
            }
        }

        float minMove = 0f;
        public int iters = 10;


        void FixedUpdate()
        {
            radius = Mathf.Max(0, radius);
            float actualRadius = radius + Mathf.Max(myRigidbody.radius + other.radius);
            for (int i = 0; i < iters; i++)
            {

                Vector2 newPos = (-myRigidbody.position + other.position).normalized * actualRadius + myRigidbody.position;


                float distanceToMove = Vector2.Distance(newPos, other.position);

                float distanceMoving = distanceToMove;
                if (distanceToMove > minMove)
                {

                    if (distanceToMove > moveDist)
                    {
                        distanceMoving = moveDist;
                    }





                    Vector2 moveDir = (newPos - other.position).normalized;

                    myRigidbody.position -= moveDir * distanceMoving / 2.0f;

                    other.position += moveDir * distanceMoving / 2.0f;

                    myRigidbody.FixCollisions();
                    other.FixCollisions();



                    Vector3 otherVelInDir = other.VectorProjection(other.velocity, (myRigidbody.position - other.position).normalized);
                    Vector3 myVelInDir = other.VectorProjection(myRigidbody.velocity, (myRigidbody.position - other.position).normalized);

                    Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;
                    other.velocity = other.velocity - otherVelInDir + avgVelInDir;
                    myRigidbody.velocity = myRigidbody.velocity - myVelInDir + avgVelInDir;
                }
            }
        }
    }
}