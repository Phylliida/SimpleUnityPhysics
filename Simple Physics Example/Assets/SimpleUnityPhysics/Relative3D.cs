using UnityEngine;
using System.Collections;
namespace SimpleUnityPhysics
{
    public class Relative3D : MonoBehaviour
    {



        public SimpleRigidbody3D otherClose;
        public SimpleRigidbody3D otherFar;
        SimpleRigidbody3D myRigidbody;
        public bool autoCalculateRadius = true;
        public float radius = 1.0f;

        SimplePhysics time;


        public void Awake()
        {
            time = FindObjectOfType<SimplePhysics>();
            myRigidbody = GetComponent<SimpleRigidbody3D>();
        }

        bool added = false;
        // Use this for initialization
        public void Start()
        {
            if (autoCalculateRadius)
            {
                radius = Vector3.Distance(myRigidbody.tmpPosition, otherClose.tmpPosition);
            }

            if (!added) { time.AddMeToTickHandler(this, UpdateMe); added = true; }
        }

        float minMove = 0f;
        float moveDist = 1.0f;


        // Update is called once per frame
        void UpdateMe()
        {
            for (int i = 0; i < time.jointIters; i++)
            {

                Vector3 newPos = (otherClose.tmpPosition - otherFar.tmpPosition).normalized * radius + otherClose.tmpPosition;


                float distanceToMove = Vector3.Distance(newPos, myRigidbody.tmpPosition);

                float distanceMoving = distanceToMove;
                bool retry = false;
                if (distanceToMove > minMove)
                {

                    if (distanceToMove > moveDist)
                    {
                        distanceMoving = moveDist;
                        retry = true;
                    }





                    Vector3 moveDir = (newPos - myRigidbody.tmpPosition).normalized;

                    otherClose.tmpPosition -= moveDir * distanceMoving / 2.0f;

                    myRigidbody.tmpPosition += moveDir * distanceMoving / 2.0f;

                    otherFar.FixCollisions();
                    myRigidbody.FixCollisions();


                    //otherClose.angularVelocity = angularVelocityScale * Vector3.Cross(-(otherClose.position - myRigidbody.position), myRigidbody.VectorRejection(new Vector3(0, -myRigidbody.gravity), -(otherClose.position - myRigidbody.position))).z;
                    //myRigidbody.angularVelocity = angularVelocityScale* Vector3.Cross(-(otherClose.position - myRigidbody.position), -myRigidbody.VectorRejection(new Vector3(0, -otherClose.gravity), -(otherClose.position - myRigidbody.position).normalized)).z;


                    Vector3 otherVelInDir = SimpleRigidbody3D.VectorProjection(myRigidbody.velocity, (otherFar.tmpPosition - myRigidbody.tmpPosition).normalized);
                    Vector3 myVelInDir = SimpleRigidbody3D.VectorProjection(otherFar.velocity, (otherFar.tmpPosition - myRigidbody.tmpPosition).normalized);

                    Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;
                    myRigidbody.velocity = myRigidbody.velocity - otherVelInDir + avgVelInDir;
                    otherFar.velocity = otherFar.velocity - myVelInDir + avgVelInDir;


                    //Vector2 avgVelocity = (otherClose.velocity + other.velocity) / 2.0f;

                    //otherClose.velocity = avgVelocity;
                    //other.velocity = avgVelocity;
                }

                if (!retry)
                {
                    break;
                }
            }
        }


        static Material lineMaterial;
        static void CreateLineMaterial()
        {
            if (!lineMaterial)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                lineMaterial = new Material(shader);
                lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                lineMaterial.SetInt("_ZWrite", 0);
            }
        }

        // Will be called after all regular rendering is done
        public void OnRenderObject()
        {
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            //GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            //GL.MultMatrix(transform.localToWorldMatrix);

            // Draw lines
            GL.Begin(GL.LINES);
            GL.Color(new Color(0, 0, 0.6f, 1));
            // One vertex at transform position
            Vector3 me = myRigidbody.transform.position;
            Vector3 ot = otherFar.transform.position;
            GL.Vertex3(me.x, me.y, me.z);
            // Another vertex at edge of circle
            GL.Vertex3(ot.x, ot.y, ot.z);
            GL.End();
            //GL.PopMatrix();
        }
    }
}