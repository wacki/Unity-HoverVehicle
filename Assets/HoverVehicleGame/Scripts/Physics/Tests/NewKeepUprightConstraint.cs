using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    public class NewKeepUprightConstraint : MonoBehaviour
    {
        // whether or not to use height probe rays to calculate our own "surface normal"
        public bool useHeightProbe = false;

        // should we smooth the current surface normal?
        public bool smoothGoalNormal = false;
        public float normalSmoothTime;

        // rotate around a pivot to keep upright
        public bool usePivot = false;
        public Vector3 pivotPosition;

        public float tempK = 0.2f;
        public float tempD = 0.8f;
        
        public Vector3 goalUpDir = Vector3.up;

        private Vector3 _goalUpDir = Vector3.up;

        private Rigidbody _rb;

        public void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            UpdateUprightConstraint();
        }

        private Vector3 _smoothNormalVel;
        private void UpdateGoalUp()
        {
            if (smoothGoalNormal)
                _goalUpDir = Vector3.SmoothDamp(_goalUpDir, goalUpDir, ref _smoothNormalVel, normalSmoothTime);
            else
                _goalUpDir = goalUpDir;
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
            _rb.AddTorque(torque * _rb.mass);

            if(usePivot)
            {
                // todo: ...
            }
                
        }
    }

}