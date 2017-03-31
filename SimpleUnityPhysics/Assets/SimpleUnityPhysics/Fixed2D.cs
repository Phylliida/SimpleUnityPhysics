using UnityEngine;
namespace SimpleUnityPhysics
{
    public class Fixed2D : MonoBehaviour
    {
        public SimpleRigidbody2D other;
        SimpleRigidbody2D myRigidbody;
        Vector2 relativePos;
        public float moveDist = 1f;
        
        void Start()
        {
            myRigidbody = GetComponent<SimpleRigidbody2D>();
            relativePos = transform.worldToLocalMatrix.MultiplyPoint(other.position);
        }

        float minMove = 0.0f;
        public int iters = 10;


        void FixedUpdate()
        {

            for (int i = 0; i < iters; i++)
            {

                Vector2 newPos = transform.localToWorldMatrix.MultiplyPoint(relativePos);


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


                    Vector2 avgVelocity = (myRigidbody.velocity + other.velocity) / 2.0f;

                    myRigidbody.velocity = avgVelocity;
                    other.velocity = avgVelocity;
                }



            }
        }
    }
}
