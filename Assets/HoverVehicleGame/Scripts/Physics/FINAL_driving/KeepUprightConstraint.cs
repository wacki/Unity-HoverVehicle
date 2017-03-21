using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    public class KeepUprightConstraint : MonoBehaviour
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

        public bool constrainToMaxAngle;
        public float maxAngleOffset;

        // smoothed GoalUpDir
        private Vector3 _smoothedGoalUpDir = Vector3.up;

        private Rigidbody _rb;

        public void Start()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            UpdateGoalUp();
            UpdateUprightConstraint();
        }

        private Vector3 _smoothNormalVel;
        private void UpdateGoalUp()
        {
            goalUpDir.Normalize();

            if (smoothGoalNormal)
                _smoothedGoalUpDir = Vector3.SmoothDamp(_smoothedGoalUpDir, goalUpDir, ref _smoothNormalVel, normalSmoothTime);
            else
                _smoothedGoalUpDir = goalUpDir;
        }

        private void UpdateUprightConstraint()
        {
            var up = transform.up;

            Quaternion deltaRot = Quaternion.FromToRotation(_smoothedGoalUpDir, up);

            if(constrainToMaxAngle)
            {
                float angle;
                Vector3 axis;
                deltaRot.ToAngleAxis(out angle, out axis);
                if (angle > maxAngleOffset)
                {
                    transform.rotation = Quaternion.AngleAxis(angle - maxAngleOffset, -axis) * transform.rotation;
                    deltaRot = Quaternion.FromToRotation(_smoothedGoalUpDir, transform.up);

                    var newAngularVel = Vector3.Project(_rb.angularVelocity, axis.normalized);
                    _rb.angularVelocity = _rb.angularVelocity - newAngularVel;
                }
            }

            Vector3 eulerDelta = deltaRot.eulerAngles;

            //Debug.Log("Euler Delta" + eulerDelta);

            // fuck unity for not using negative angles
            eulerDelta.x = (eulerDelta.x > 180) ? eulerDelta.x - 360 : eulerDelta.x;
            eulerDelta.y = (eulerDelta.y > 180) ? eulerDelta.y - 360 : eulerDelta.y;
            eulerDelta.z = (eulerDelta.z > 180) ? eulerDelta.z - 360 : eulerDelta.z;

            Vector3 projectedAngularVelocity = Vector3.Project(_rb.angularVelocity, transform.up);
            projectedAngularVelocity = _rb.angularVelocity - projectedAngularVelocity;

            Vector3 torque = -tempK * eulerDelta - tempD * projectedAngularVelocity;
            //torque.y = 0.0f;
            _rb.AddTorque(torque * _rb.mass);

            if (usePivot)
            {
                // todo: ...
            }

        }

        private void OnDrawGizmos()
        {

        }
    }

}