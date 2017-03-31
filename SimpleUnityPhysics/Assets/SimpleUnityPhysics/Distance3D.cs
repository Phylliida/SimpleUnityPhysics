using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace SimpleUnityPhysics
{
    public class Distance3D : MonoBehaviour
    {
        public bool creatureUsingMe = false;
        public SimpleRigidbody3D other;
        SimpleRigidbody3D myRigidbody;
        public float distance = 1.0f;

        public Vector3 direction;

        SimplePhysics time;

        bool added = false;

        public void Awake()
        {
            time = FindObjectOfType<SimplePhysics>();
            myRigidbody = GetComponent<SimpleRigidbody3D>();
        }

        public void Start()
        {
            distance = Vector3.Distance(myRigidbody.tmpPosition, other.tmpPosition);

            direction = -(myRigidbody.tmpPosition - other.tmpPosition).normalized;

            if (!added) { time.AddMeToTickHandler(this, UpdateMe); added = true; }
        }
        float minMove = 1.0f;

        
        void UpdateMe()
        {
            distance = Mathf.Max(0, distance);

            float actualRadius = distance + other.radius + myRigidbody.radius;

            // Apply distance constraint
            for (int i = 0; i < time.jointIters; i++)
            {

                Vector3 newPos = (-myRigidbody.tmpPosition + other.tmpPosition).normalized * actualRadius + myRigidbody.tmpPosition;


                float distanceToMove = Vector3.Distance(newPos, other.tmpPosition);

                float distanceMoving = distanceToMove;
                if (distanceToMove > minMove)
                {

                    if (distanceToMove > time.dt * 100.0f)
                    {
                        distanceMoving = time.dt * 100.0f;
                    }



                    Vector3 moveDir = (newPos - other.tmpPosition).normalized;

                    myRigidbody.tmpPosition -= moveDir * distanceMoving / 2.0f;

                    other.tmpPosition += moveDir * distanceMoving / 2.0f;

                    myRigidbody.FixCollisions();
                    other.FixCollisions();


                    Vector3 otherVelInDir = SimpleRigidbody3D.VectorProjection(other.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);
                    Vector3 myVelInDir = SimpleRigidbody3D.VectorProjection(myRigidbody.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);

                    Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;
                    other.velocity = other.velocity - otherVelInDir + avgVelInDir;
                    myRigidbody.velocity = myRigidbody.velocity - myVelInDir + avgVelInDir;
                    myRigidbody.velocity -= moveDir * distanceMoving / 2.0f;
                    other.velocity += moveDir * distanceMoving / 2.0f;
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
            GL.Color(new Color(0, 0, 0, 1));
            // One vertex at transform position
            Vector3 me = myRigidbody.transform.position;
            Vector3 ot = other.transform.position;
            GL.Vertex3(me.x, me.y, me.z);
            // Another vertex at edge of circle
            GL.Vertex3(ot.x, ot.y, ot.z);
            GL.End();
            //GL.PopMatrix();
        }
    }
}