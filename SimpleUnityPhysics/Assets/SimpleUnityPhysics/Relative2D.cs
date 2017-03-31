using UnityEngine;
using System.Collections;
namespace SimpleUnityPhysics
{
    public class Relative2D : MonoBehaviour
    {



        public SimpleRigidbody2D otherClose;
        public SimpleRigidbody2D otherFar;
        SimpleRigidbody2D myRigidbody;
        public bool autoCalculateRadius = true;
        public float radius = 1.0f;
        public float moveDist = 1f;

        // Use this for initialization
        void Start()
        {
            myRigidbody = GetComponent<SimpleRigidbody2D>();
            if (autoCalculateRadius)
            {
                radius = Vector3.Distance(myRigidbody.position, otherClose.position);
            }
        }

        float minMove = 0f;
        public int iters = 10;


        // Update is called once per frame
        void FixedUpdate()
        {

            for (int i = 0; i < iters; i++)
            {

                Vector2 newPos = (otherClose.position - otherFar.position).normalized * radius + otherClose.position;


                float distanceToMove = Vector2.Distance(newPos, myRigidbody.position);

                float distanceMoving = distanceToMove;
                if (distanceToMove > minMove)
                {

                    if (distanceToMove > moveDist)
                    {
                        distanceMoving = moveDist;
                    }





                    Vector2 moveDir = (newPos - myRigidbody.position).normalized;

                    otherClose.position -= moveDir * distanceMoving / 2.0f;

                    myRigidbody.position += moveDir * distanceMoving / 2.0f;

                    otherFar.FixCollisions();
                    myRigidbody.FixCollisions();


                    //otherClose.angularVelocity = angularVelocityScale * Vector3.Cross(-(otherClose.position - myRigidbody.position), myRigidbody.VectorRejection(new Vector3(0, -myRigidbody.gravity), -(otherClose.position - myRigidbody.position))).z;
                    //myRigidbody.angularVelocity = angularVelocityScale* Vector3.Cross(-(otherClose.position - myRigidbody.position), -myRigidbody.VectorRejection(new Vector3(0, -otherClose.gravity), -(otherClose.position - myRigidbody.position).normalized)).z;


                    Vector3 otherVelInDir = myRigidbody.VectorProjection(myRigidbody.velocity, (otherFar.position - myRigidbody.position).normalized);
                    Vector3 myVelInDir = myRigidbody.VectorProjection(otherFar.velocity, (otherFar.position - myRigidbody.position).normalized);

                    Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;
                    myRigidbody.velocity = myRigidbody.velocity - otherVelInDir + avgVelInDir;
                    otherFar.velocity = otherFar.velocity - myVelInDir + avgVelInDir;


                    //Vector2 avgVelocity = (otherClose.velocity + other.velocity) / 2.0f;

                    //otherClose.velocity = avgVelocity;
                    //other.velocity = avgVelocity;
                }



            }
        }


    }
}