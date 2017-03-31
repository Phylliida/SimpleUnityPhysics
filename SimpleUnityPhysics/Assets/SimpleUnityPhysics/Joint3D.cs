using UnityEngine;
using System.Collections;
namespace SimpleUnityPhysics
{
    public enum JointType
    {
        Fixed,
        Free
    }

    public class Joint3D : MonoBehaviour
    {

        public SimpleRigidbody3D left;

        public SimpleRigidbody3D right;

        [HideInInspector]
        public SimpleRigidbody3D me;

        public JointType jointType;

        void Awake()
        {
            me = GetComponent<SimpleRigidbody3D>();
        }

        void Start()
        {

        }

        void Update()
        {

        }
    }
}