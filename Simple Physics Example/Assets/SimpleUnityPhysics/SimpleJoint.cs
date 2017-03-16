using UnityEngine;
using System.Collections;

namespace SimpleUnityPhysics
{
    public class SimpleJoint : MonoBehaviour
    {


        public enum SimpleJointType
        {
            BallJoint,
            FixedJoint,
            AngleJoint,
            HingeJoint
        }

        public SimpleJointType jointType;

        [HideInInspector]
        public float prevDistEndpoints;

        [HideInInspector]
        public float initialDistEndpoints;

        public SimpleRigidbody3D left;
        public SimpleRigidbody3D right;

        public float angleJointMinAngle = 10.0f;

        [HideInInspector]
        public Bone leftBone;

        [HideInInspector]
        public Bone rightBone;

        [HideInInspector]
        public SimpleRigidbody3D myRigidbody;

        [HideInInspector]
        public float myDistFromHelper;

        [HideInInspector]
        public float leftDistFromHelper;

        [HideInInspector]
        public float rightDistFromHelper;

        [HideInInspector]
        public float myPrevDistFromHelper;

        [HideInInspector]
        public float leftPrevDistFromHelper;

        [HideInInspector]
        public float rightPrevDistFromHelper;

        [HideInInspector]
        public Vector3 helperPos;

        [HideInInspector]
        public Vector3 helperVelocity;

        public GameObject helper;

        void Awake()
        {

            helper = new GameObject();

            prevDistEndpoints = Vector3.Distance(left.transform.position, right.transform.position);
            initialDistEndpoints = Vector3.Distance(left.transform.position, right.transform.position);
            myRigidbody = GetComponent<SimpleRigidbody3D>();

            helperVelocity = Vector3.zero;


            Vector3 dVector = left.transform.position - right.transform.position;

            // Find a vector that is perpendicular to (left - right) so the helper is useful if left me and right are collinear.
            helperPos = Vector3.Cross(dVector, new Vector3(dVector.x + 1.0f, dVector.y, dVector.z)).normalized * 2.0f + myRigidbody.transform.position;

            leftDistFromHelper = Vector3.Distance(left.transform.position, helperPos);
            myDistFromHelper = Vector3.Distance(myRigidbody.transform.position, helperPos);
            rightDistFromHelper = Vector3.Distance(right.transform.position, helperPos);

            leftPrevDistFromHelper = Vector3.Distance(left.transform.position, helperPos);
            myPrevDistFromHelper = Vector3.Distance(myRigidbody.transform.position, helperPos);
            rightPrevDistFromHelper = Vector3.Distance(right.transform.position, helperPos);


            Bone[] leftBones = left.GetComponents<Bone>();
            Bone[] myBones = myRigidbody.GetComponents<Bone>();
            Bone[] rightBones = right.GetComponents<Bone>();

            foreach (Bone bone in leftBones)
            {
                if (bone.other == myRigidbody)
                {
                    leftBone = bone;
                    break;
                }
            }

            foreach (Bone bone in rightBones)
            {
                if (bone.other == myRigidbody)
                {
                    rightBone = bone;
                    break;
                }
            }


            foreach (Bone bone in myBones)
            {
                if (bone.other == left && leftBone == null)
                {
                    leftBone = bone;
                }
                if (bone.other == right && rightBone == null)
                {
                    rightBone = bone;
                }
            }

            if (leftBone == null)
            {
                Debug.LogError(name + " does not have a left bone for its joint between (left: " + left.name + ", right: " + right.name + ")");
            }
            else if (rightBone == null)
            {
                Debug.LogError(name + " does not have a right bone for its joint between (left: " + left.name + ", right: " + right.name + ")");
            }

        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}