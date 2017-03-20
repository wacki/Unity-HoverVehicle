using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    public class KeepUprightConstraintOLD : MonoBehaviour
    {

        public float tempK = 0.2f;
        public float tempD = 0.8f;

        public Vector3 goalUpDir = Vector3.up;

        private Rigidbody _rb;

        public void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            UpdateUprightConstraint();
        }

        private void UpdateUprightConstraint()
        {
            var up = transform.up;

            Quaternion deltaRot = Quaternion.FromToRotation(goalUpDir, up);            
            Vector3 eulerDelta = deltaRot.eulerAngles;
            // fuck unity for not using negative angles
            eulerDelta.x = (eulerDelta.x > 180) ? eulerDelta.x - 360 : eulerDelta.x;
            eulerDelta.y = (eulerDelta.y > 180) ? eulerDelta.y - 360 : eulerDelta.y;
            eulerDelta.z = (eulerDelta.z > 180) ? eulerDelta.z - 360 : eulerDelta.z;

            Vector3 torque = -tempK * eulerDelta - tempD * _rb.angularVelocity;
            //torque.y = 0.0f;
            _rb.AddTorque(torque);
        }
    }

}