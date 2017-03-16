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

        // Use this for initialization
        public void Start()
        {
            //relativePosFromMe = Matrix4x4.TRS(myRigidbody.tmpPosition, myRigidbody.tmpRotation, Vector3.one).inverse.MultiplyPoint(other.tmpPosition);
            //relativePosFromOther = Matrix4x4.TRS(other.tmpPosition, other.tmpRotation, Vector3.one).inverse.MultiplyPoint(myRigidbody.tmpPosition);
            distance = initialDistance;
            prevDistance = distance;
            if (!added) { time.AddMeToTickHandler(this, UpdateMe); added = true; }
        }

        // Update is called once per frame
        void UpdateMe()
        {
            return;
            /*
            distance = Mathf.Max(0, distance);
            if (direction.magnitude == 0.0)
            {
                direction = Vector3.one;
            }

            float actualDistance = distance + other.radius + myRigidbody.radius;

            direction = direction.normalized;



            // Apply direction constraint
            for (int i = 0; i < time.jointIters; i++)
            {
                Vector3 goalMe = other.tmpPosition - direction * actualDistance;
                Vector3 meOffset = myRigidbody.tmpPosition - goalMe;

                Vector3 goalOther = myRigidbody.tmpPosition + direction* actualDistance;
                Vector3 otherOffset = other.tmpPosition - goalOther;



                float distanceToMove = (meOffset.magnitude + otherOffset.magnitude) / 2.0f;

                if (distanceToMove > minMove)
                {
                    distanceToMove = minMove;
                }


                //averageOffset = averageOffset.normalized * distanceToMove;


                myRigidbody.tmpPosition -= meOffset.normalized*distanceToMove / 2.0f;

                other.tmpPosition -= otherOffset.normalized*distanceToMove / 2.0f;

                myRigidbody.FixCollisions();
                other.FixCollisions();


                Vector3 otherVelInDir = other.VectorProjection(other.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);
                Vector3 myVelInDir = other.VectorProjection(myRigidbody.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);

                Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;
                //other.velocity = other.velocity - otherVelInDir + avgVelInDir;
                //myRigidbody.velocity = myRigidbody.velocity - myVelInDir + avgVelInDir;
                //myRigidbody.velocity += meOffset / 2.0f;
                //other.velocity += otherOffset / 2.0f;
            }

            /*
            // Apply distance constraint
            for (int i = 0; i < time.jointIters; i++)
            {

                Vector3 newPos = (-myRigidbody.tmpPosition + other.tmpPosition).normalized * actualRadius + myRigidbody.tmpPosition;


                float distanceToMove = Vector3.Distance(newPos, other.tmpPosition);

                float distanceMoving = distanceToMove;
                if (distanceToMove > minMove)
                {

                    if (distanceToMove > time.dt*100.0f)
                    {
                        distanceMoving = time.dt * 100.0f;
                    }





                    Vector3 moveDir = (newPos - other.tmpPosition).normalized;

                    myRigidbody.tmpPosition -= moveDir * distanceMoving/2.0f;

                    other.tmpPosition += moveDir * distanceMoving / 2.0f;

                    myRigidbody.FixCollisions();
                    other.FixCollisions();

                    //myRigidbody.angularVelocity = angularVelocityScale * Vector3.Cross(-(myRigidbody.position - other.position), other.VectorRejection(new Vector3(0, -other.gravity), -(myRigidbody.position - other.position))).z;
                    //other.angularVelocity = angularVelocityScale* Vector3.Cross(-(myRigidbody.position - other.position), -other.VectorRejection(new Vector3(0, -myRigidbody.gravity), -(myRigidbody.position - other.position).normalized)).z;


                    Vector3 otherVelInDir = other.VectorProjection(other.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);
                    Vector3 myVelInDir = other.VectorProjection(myRigidbody.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);

                    Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;
                    other.velocity = other.velocity - otherVelInDir + avgVelInDir;
                    myRigidbody.velocity = myRigidbody.velocity - myVelInDir + avgVelInDir;
                    myRigidbody.velocity -= moveDir * distanceMoving / 2.0f;
                    other.velocity += moveDir * distanceMoving / 2.0f;


                    //Vector2 avgVelocity = (myRigidbody.velocity + other.velocity) / 2.0f;

                    //myRigidbody.velocity = avgVelocity;
                    //other.velocity = avgVelocity;
                }



            }
            */

            /*
            for (int i = 0; i < time.jointIters; i++)
            {

                bool retry = false;

                Vector3 otherGoalPos = Matrix4x4.TRS(myRigidbody.tmpPosition, myRigidbody.tmpRotation, Vector3.one).MultiplyPoint(relativePosFromMe);
                Vector3 myGoalPos = Matrix4x4.TRS(other.tmpPosition, other.tmpRotation, Vector3.one).MultiplyPoint(relativePosFromOther);


                Vector3 actualMyGoalPos = (myGoalPos + myRigidbody.tmpPosition) / 2.0f;
                Vector3 actualOtherGoalPos = (otherGoalPos + other.tmpPosition) / 2.0f;


                Vector3 myRigidbodyDisplacement = actualMyGoalPos - myRigidbody.tmpPosition;
                Vector3 otherRigidbodyDisplacement = actualOtherGoalPos - other.tmpPosition;


                myRigidbody.tmpPosition = actualMyGoalPos;
                other.tmpPosition = actualOtherGoalPos;



                Vector3 curPos = myRigidbody.tmpPosition;
                Vector3 curOtherPos = other.tmpPosition;

                myRigidbody.FixCollisions();
                other.FixCollisions();

                Vector3 groundMeDisplacement = curPos - myRigidbody.tmpPosition;
                Vector3 groundOtherDisplacement = curOtherPos - other.tmpPosition;



                // Velocities going along bone
                Vector3 myVelInDir = SimpleRigidbody3D.VectorProjection(myRigidbody.velocity, (other.tmpPosition - myRigidbody.tmpPosition).normalized);
                Vector3 otherVelInDir = SimpleRigidbody3D.VectorProjection(other.velocity, (other.tmpPosition - myRigidbody.tmpPosition).normalized);

                Vector3 avgInDir = (myVelInDir + otherVelInDir) / 2.0f;


                // Velocities perpendicular to bone
                Vector3 myVelOutDir = SimpleRigidbody3D.VectorRejection(myRigidbody.velocity, (other.tmpPosition - myRigidbody.tmpPosition).normalized);
                Vector3 otherVelOutDir = SimpleRigidbody3D.VectorRejection(other.velocity, (other.tmpPosition - myRigidbody.tmpPosition).normalized);

                Vector3 myNotSharedVel = myVelOutDir - SimpleRigidbody3D.VectorProjection(otherVelOutDir, myVelOutDir.normalized);
                Vector3 otherNotSharedVel = otherVelOutDir - SimpleRigidbody3D.VectorProjection(myVelOutDir, otherVelOutDir.normalized);


                // Shared direction
                Vector3 sharedDirection = (myVelOutDir + otherVelOutDir) / 2.0f;

                Vector3 a = SimpleRigidbody3D.VectorProjection(myVelOutDir, otherVelOutDir.normalized);
                Vector3 b = SimpleRigidbody3D.VectorProjection(otherVelOutDir, myVelOutDir.normalized);
                Vector3 shared = (a + b) / 2.0f;

                Vector3 myExtraVector = SimpleRigidbody3D.VectorRejection(myVelOutDir, otherVelOutDir.normalized);
                Vector3 otherExtraVector = SimpleRigidbody3D.VectorRejection(otherVelOutDir, myVelOutDir.normalized);


                // Get velocity that is shared among them
                Vector3 mySharedVel = myVelOutDir - myNotSharedVel;
                Vector3 otherSharedVel = otherVelOutDir - otherNotSharedVel;


                myRigidbody.velocity = a + avgInDir;
                other.velocity = b + avgInDir;

                float dist = (other.tmpPosition - myRigidbody.tmpPosition).magnitude;


                // - -
                // | | float

                // - |
                // | | float

                // | |
                // - - float

                // - |
                // | - inverted

                // | -
                // - | inverted

                // - | 
                // - | inverted




                myRigidbody.angularVelocity += Vector3.Cross((other.tmpPosition - myRigidbody.tmpPosition).normalized, -otherNotSharedVel) * -time.dt;
                other.angularVelocity += Vector3.Cross((myRigidbody.tmpPosition - other.tmpPosition).normalized, -myNotSharedVel)* - time.dt;


                Vector3 averageAngularVelocity = (myRigidbody.angularVelocity + other.angularVelocity)/2.0f;

                myRigidbody.angularVelocity = averageAngularVelocity;
                other.angularVelocity = averageAngularVelocity;

                //myRigidbody.velocity += otherRigidbodyDisplacement / 2.0f;
                //other.velocity += myRigidbodyDisplacement / 2.0f;
                /*
                myRigidbody.angularVelocity += Vector3.Cross((other.tmpPosition - myRigidbody.tmpPosition).normalized, otherNotSharedVelocity);
                other.angularVelocity += Vector3.Cross((myRigidbody.tmpPosition - other.tmpPosition).normalized, myNotSharedVelocity);

                Vector3 avgAngularVelocity = (myRigidbody.angularVelocity + other.angularVelocity) / 2.0f;



                * /
                if (!retry)
                {
                    //break;
                }

            }
            */
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







        // Previous bone code attempts




        /*

        //Vector3 newPos = myRigidbody.transform.localToWorldMatrix.MultiplyPoint(relativePos);















        Vector3 moveDirOther = (otherGoalPos - other.tmpPosition);
        Vector3 moveDirMe = (myGoalPos - myRigidbody.tmpPosition);
        Vector3 moveDirAverage = (moveDirOther.normalized - moveDirMe.normalized) / 2.0f;
        Vector3 centerPos = (myRigidbody.tmpPosition + other.tmpPosition) / 2.0f;


        float distanceMoving = moveDirAverage.magnitude;


        if (distanceMoving > moveDist)
        {
            //distanceMoving = moveDist;
            //retry = true;
        }
        //moveDirAverage = moveDirAverage;




        Vector3 curPos = myRigidbody.tmpPosition;
        myRigidbody.tmpPosition = centerPos+moveDirAverage.normalized* initialDistance/2.0f;

        Vector3 otherPos = other.tmpPosition;
        other.tmpPosition = centerPos - moveDirAverage.normalized * initialDistance / 2.0f;

        Vector3 dCur = curPos - myRigidbody.tmpPosition;
        Vector3 dOther = otherPos - other.tmpPosition;

        dCur = other.VectorRejection(dCur, (myRigidbody.tmpPosition - other.tmpPosition).normalized);
        dOther = other.VectorRejection(dOther, (other.tmpPosition - myRigidbody.tmpPosition).normalized);

        Vector3 combinedTorque = (dCur - dOther) / 2.0f;

        Vector3 curTorque = -combinedTorque / 2.0f;
        Vector3 otherTorque = combinedTorque / 2.0f;


        myRigidbody.FixCollisions();
        other.FixCollisions();
        //myRigidbody.angularVelocity = angularVelocityScale * Vector3.Cross(-(myRigidbody.position - other.position), other.VectorRejection(new Vector3(0, -other.gravity), -(myRigidbody.position - other.position))).z;
        //other.angularVelocity = angularVelocityScale* Vector3.Cross(-(myRigidbody.position - other.position), -other.VectorRejection(new Vector3(0, -myRigidbody.gravity), -(myRigidbody.position - other.position).normalized)).z;


        Vector3 avgVelocity = (myRigidbody.velocity + other.velocity) / 2.0f;

        //myRigidbody.velocity = avgVelocity;
        //other.velocity = avgVelocity;


        / *
        Vector3 myLocalVelocity = Matrix4x4.TRS(myRigidbody.tmpPosition, myRigidbody.tmpRotation, Vector3.one).inverse.MultiplyPoint(myRigidbody.velocity);
        Vector3 otherLocalVelocity = Matrix4x4.TRS(other.tmpPosition, other.tmpRotation, Vector3.one).inverse.MultiplyPoint(other.velocity);

        Vector3 myLocalVelocityAlongDir = other.VectorProjection(myLocalVelocity, relativePosFromMe.normalized);
        Vector3 otherLocalVelocityAlongDir = other.VectorProjection(otherLocalVelocity, relativePosFromOther.normalized);

        Vector3 averageLocalVelocity = (myLocalVelocityAlongDir + otherLocalVelocityAlongDir) / 2.0f;

        Vector3 myNewLocalVelocity = 
        * /

        //myRigidbody.velocity += dOther;
        //other.velocity += dCur;




        // Velocities going along bone
        Vector3 otherVelInDir = other.VectorProjection(other.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);
        Vector3 myVelInDir = other.VectorProjection(myRigidbody.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);


        // This should be their shared velocity going out from the bone
        Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;



        // Velocities perpendicular to the bar
        Vector3 otherVelOutDir = other.VectorRejection(other.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);
        Vector3 myVelOutDir = other.VectorRejection(myRigidbody.velocity, -(myRigidbody.tmpPosition - other.tmpPosition).normalized);


        // Get velocity that isn't shared among them
        Vector3 myNotSharedVel = myVelOutDir - other.VectorProjection(otherVelOutDir, myVelOutDir.normalized);
        Vector3 otherNotSharedVel = otherVelOutDir - other.VectorProjection(myVelOutDir, otherVelOutDir.normalized);

        // Get velocity that is shared among them
        Vector3 mySharedVel = myVelOutDir - myNotSharedVel;
        Vector3 otherSharedVel = otherVelOutDir - otherNotSharedVel;


        myRigidbody.velocity = mySharedVel - avgVelInDir;
        other.velocity = otherSharedVel + avgVelInDir;




        // 









        //other.velocity = (other.velocity - otherVelInDir + avgVelInDir;
        //myRigidbody.velocity = myRigidbody.velocity + myVelInDir - avgVelInDir;

        /*

        Vector3 otherVelInDir = other.VectorProjection(other.velocity, -(myRigidbody.tmpPosition - other.tmpPosition).normalized);
        Vector3 myVelInDir = other.VectorProjection(myRigidbody.velocity, (myRigidbody.tmpPosition - other.tmpPosition).normalized);


        Vector3 avgVelInDir = (otherVelInDir - myVelInDir) / 2.0f;

        Vector3 newOtherVel = other.velocity - otherVelInDir - avgVelInDir;
        Vector3 newMyVel = myRigidbody.velocity - myVelInDir + avgVelInDir;
        * /

        //Vector3 ourDir = (myRigidbody.tmpPosition - other.tmpPosition).normalized;
        //myRigidbody.velocity = newMyVel;
        //other.velocity = newOtherVel;


        //Vector3 myResVelocity = 


        Vector3 myAddedAngVel = Vector3.Cross(-(myRigidbody.tmpPosition - other.tmpPosition), otherTorque);
        Vector3 otherAddedAngVel = Vector3.Cross(-(other.tmpPosition - myRigidbody.tmpPosition), curTorque);
        float maxAng = 1.0f;

        if (myAddedAngVel.magnitude > maxAng)
        {
            myAddedAngVel = myAddedAngVel.normalized * maxAng;
        }

        if (otherAddedAngVel.magnitude > maxAng)
        {
            otherAddedAngVel = otherAddedAngVel.normalized * maxAng;
        }

        //if (!retry)
        {
            myRigidbody.angularVelocity += myAddedAngVel;
            other.angularVelocity += otherAddedAngVel;

            Vector3 averageAngularVelocity = (myRigidbody.angularVelocity + other.angularVelocity)/2.0f;
            //myRigidbody.angularVelocity = averageAngularVelocity;
            //other.angularVelocity = averageAngularVelocity;
        }
        /*
        //myRigidbody.velocity = (newOtherVel + newMyVel) / 2.0f;
        //other.velocity = (newOtherVel + newMyVel) / 2.0f;


        Vector3 otherPerpendicularVel = other.VectorRejection(otherVelOutDir-myVelOutDir, -ourDir);
        Vector3 myPerpendicularVel = other.VectorRejection(-otherVelOutDir + myVelOutDir, ourDir);

       // myRigidbody.angularVelocity += otherVelInDir;
       // other.angularVelocity += myVelInDir;

        float dist = (myRigidbody.tmpPosition - other.tmpPosition).magnitude;

        // This needs to be fixed to "actual # of iters done", but that needs to be done in hindsight and is gross so this approximation should be fine
        myRigidbody.angularVelocity += Vector3.Cross(ourDir, otherPerpendicularVel) / (dist) / (dist);
        other.angularVelocity += Vector3.Cross(-ourDir, myPerpendicularVel) / (dist) / (dist);


        Vector3 averageAngularVelocity = (myRigidbody.angularVelocity + other.angularVelocity)/2.0f;
        //myRigidbody.angularVelocity = averageAngularVelocity;
        //other.angularVelocity = averageAngularVelocity;


        //other.velocity = (myRigidbody.velocity + other.velocity) / 2.0f;

        //Vector3 avgVelInDir = (otherVelInDir + myVelInDir) / 2.0f;
        //other.velocity = other.velocity - otherVelInDir + avgVelInDir;
        //myRigidbody.velocity = myRigidbody.velocity - myVelInDir + avgVelInDir;
        //myRigidbody.velocity -= moveDir * distanceMoving / 2.0f;
        //other.velocity += moveDir * distanceMoving / 2.0f;
        */
    }
}