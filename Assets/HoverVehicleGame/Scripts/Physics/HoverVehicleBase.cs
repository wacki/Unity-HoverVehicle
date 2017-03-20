using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    public abstract class HoverVehicleBase : MonoBehaviour
    {
        public bool inputActive = false;

        public float forwardAcceleration = 100.0f;
        public float backwardAcceleration = 50.0f;

        public float brakeAcceleration = 1000.0f;

        public float thrusterForce = 1000;
        public float turnTorque = 100;

        public float sidewaysFrictionFactor = 0.9f;

        public float testLeanAmount = 30.0f;

        public GameObject model;

        protected Rigidbody _rb;

        public Transform centerOfMass;
        public Transform forwardThrustForcePoint;

        public Transform turnPoint;

        public float testGravity = 9.81f;

        protected virtual void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            if (centerOfMass != null)
                _rb.centerOfMass = centerOfMass.localPosition;
        }

        public Vector3 groundNormal { get { return GetGroundNormal(); } }
        public bool movingForward { get { return IsMovingForward(); } }
        public bool isGrounded { get { return IsGrounded(); } }


        protected abstract Vector3 GetGroundNormal();
        protected abstract bool IsGrounded();
        protected virtual bool IsMovingForward()
        {
            var velDot = Vector3.Dot(_rb.velocity.normalized, transform.forward);
            return velDot > 0.0f;
        }

        // todo: distinguish between brake force and throttle forward and backwards!
        protected virtual void Update()
        {

        }

        protected virtual void FixedUpdate()
        {
            HandleUserInput();
            ApplyFrictionForce();

            var normal = groundNormal;
            if (!isGrounded)
                normal = Vector3.up;

            _rb.AddForce(normal * -testGravity, ForceMode.Acceleration);
        }

        protected virtual void HandleUserInput()
        {

            if (!inputActive)
                return;

            var v = Input.GetAxis("Vertical");
            var h = Input.GetAxis("Horizontal");
            var throttle = Input.GetAxis("Throttle");
            var brake = Input.GetAxis("Brake");

            var mainThrusterInput = throttle + brake;
            Move(mainThrusterInput);
            Turn(h);
        }

        protected void Move(float moveValue)
        {
            Vector3 tangentForward = Vector3.ProjectOnPlane(transform.forward, GetGroundNormal());
            tangentForward.Normalize();

            Vector3 force = tangentForward * thrusterForce * moveValue;

            if (forwardThrustForcePoint == null)
                _rb.AddForce(force);
            else
                _rb.AddForceAtPosition(force, forwardThrustForcePoint.position);


        }

        protected void Turn(float turnValue)
        {
            Vector3 tangentRight = Vector3.ProjectOnPlane(transform.right, GetGroundNormal());
            tangentRight.Normalize();

            if (turnPoint == null)
                _rb.AddTorque(transform.rotation * new Vector3(0, turnValue * turnTorque, 0));
            else
                _rb.AddForceAtPosition(tangentRight * turnValue * turnTorque, turnPoint.position);

            if (model != null)
                model.transform.localRotation = Quaternion.Euler(0, 0, -turnValue * testLeanAmount);



        }

        protected virtual void ApplyFrictionForce()
        {
            if (!isGrounded)
                return;

            Vector3 tangentVelocity = Vector3.ProjectOnPlane(_rb.velocity, GetGroundNormal());

            Debug.DrawLine(transform.position, transform.position + tangentVelocity);

            Vector3 localTangentVelocity = Quaternion.Inverse(transform.rotation) * tangentVelocity;
            localTangentVelocity.z = 0.0f;
            localTangentVelocity.y = 0.0f;

            

            tangentVelocity = transform.rotation * -localTangentVelocity;


            Debug.DrawLine(transform.position, transform.position + tangentVelocity.normalized * Mathf.Clamp(tangentVelocity.magnitude, 0, 4), Color.red);

            _rb.AddForce(tangentVelocity * sidewaysFrictionFactor, ForceMode.Impulse);
        }
    }
}