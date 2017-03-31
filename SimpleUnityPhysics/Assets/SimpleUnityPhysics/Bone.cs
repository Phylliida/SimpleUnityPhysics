using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace SimpleUnityPhysics
{
    public class Bone : MonoBehaviour
    {
        public bool creatureUsingMe = false;
        public SimpleRigidbody3D other;

        [HideInInspector]
        public SimpleRigidbody3D myRigidbody;

        SimplePhysics time;

        bool added = false;


        public void Awake()
        {
            time = FindObjectOfType<SimplePhysics>();
            myRigidbody = GetComponent<SimpleRigidbody3D>();
            initialDistance = Vector3.Distance(myRigidbody.transform.position, other.transform.position);
        }

        [HideInInspector]
        public float initialDistance = 0.0f;

        public float prevDistance;
        public float distance;
        
        public void Start()
        {
            distance = initialDistance;
            prevDistance = distance;
            if (!added) { time.AddMeToTickHandler(this, UpdateMe); added = true; }
        }
        
        void UpdateMe()
        {

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