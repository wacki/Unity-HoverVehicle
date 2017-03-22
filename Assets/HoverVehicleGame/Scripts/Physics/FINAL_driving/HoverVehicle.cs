using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    public class HoverVehicle : MonoBehaviour
    {
        #region public fields

        public float forwardAcceleration = 100.0f;
        public float backwardAcceleration = 50.0f;
        public float turnAcceleration = 50.0f;
        public float brakeAcceleration = 1000.0f;

        public Transform centerOfMass;
        public Transform turnPoint;

        public float groundedGravity = 9.81f;
        public float airborneGravity = 9.81f;

        public float sidewayFrictionFactor;

        public float maxSpeedKMH = 300.0f;

        #endregion

        #region public properties

        public bool isGrounded { get { return _hoverSuspension.isGrounded; } }
        public Vector3 groundNormal { get { return _hoverSuspension.groundNormal; } }
        public Vector3 gravityDir { get { return isGrounded ? -groundNormal : -Vector3.up; } }
        public Vector3 gravity { get { return gravityDir * (isGrounded ? groundedGravity : airborneGravity); } }

        #endregion

        #region private 
        protected HoverSuspension _hoverSuspension;
        private KeepUprightConstraint _uprightConstraint;
        protected Rigidbody _rb;
        private float _curSpeed;

        private void Awake()
        {
            _hoverSuspension = GetComponent<HoverSuspension>();
            _uprightConstraint = GetComponent<KeepUprightConstraint>();
            _rb = GetComponent<Rigidbody>();

            if (centerOfMass != null)
                _rb.centerOfMass = centerOfMass.localPosition;
        }

        private void Update()
        {
            _curSpeed = MStoKMH(_rb.velocity.magnitude);
        }

        private void FixedUpdate()
        {
            HandleUserInput();
            ApplyFrictionForce();

            var keepUpright = GetComponent<KeepUprightConstraint>();
            if (keepUpright != null)
                keepUpright.goalUpDir = groundNormal;

            _rb.AddForce(gravity, ForceMode.Acceleration);
        }

        private void HandleUserInput()
        {
            var v = Input.GetAxis("Vertical");
            var h = Input.GetAxis("Horizontal");
            var throttle = Input.GetAxis("Throttle");
            var brake = Input.GetAxis("Brake");

            var mainThrusterInput = throttle + brake;
            Move(mainThrusterInput);
            Turn(h);
        }

        private Vector3 ProjectOnGround(Vector3 vec)
        {
            return Vector3.ProjectOnPlane(vec, groundNormal);
        }

        private void Move(float moveValue)
        {
            if (_curSpeed > maxSpeedKMH)
                return;

            Vector3 tangentForward = ProjectOnGround(transform.forward);
            tangentForward.Normalize();


            // TODO: choose correct acceleration values based on if we're going forward or not
            //if (moveValue > 0)
            //    acc *= forwardAcceleration;
            //else
            //    acc *= backwardAcceleration;

            Vector3 acc = tangentForward * moveValue * forwardAcceleration;
            _rb.AddForce(acc, ForceMode.Acceleration);
        }

        private void Turn(float turnValue)
        {
            Vector3 tangentRight = ProjectOnGround(transform.right);
            tangentRight.Normalize();

            if (turnPoint == null)
                _rb.AddTorque(new Vector3(0, turnValue * turnAcceleration, 0), ForceMode.Acceleration);
            else
                _rb.AddForceAtPosition(tangentRight * turnValue * turnAcceleration, turnPoint.position, ForceMode.Acceleration);

        }

        protected virtual void ApplyFrictionForce()
        {
            if (!isGrounded)
                return;

            Vector3 tangentVelocity = ProjectOnGround(_rb.velocity);

            Debug.DrawLine(transform.position, transform.position + tangentVelocity);

            Vector3 localTangentVelocity = Quaternion.Inverse(transform.rotation) * tangentVelocity;
            localTangentVelocity.z = 0.0f;
            localTangentVelocity.y = 0.0f;


            tangentVelocity = transform.rotation * -localTangentVelocity;


            Debug.DrawLine(transform.position, transform.position + tangentVelocity.normalized * Mathf.Clamp(tangentVelocity.magnitude, 0, 4), Color.red);

            _rb.AddForce(tangentVelocity * sidewayFrictionFactor, ForceMode.Impulse);
        }

        protected float MStoKMH(float speed)
        {
            return speed * 3.6f;
        }
        protected float KMHtoMS(float speed)
        {
            return speed / 3.6f;
        }

        void OnGUI()
        {
            GUILayout.Label(_curSpeed.ToString());
        }
        #endregion
    }

}