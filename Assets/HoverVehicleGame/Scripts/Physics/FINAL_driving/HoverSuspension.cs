using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HoverRacingGame
{
    public class HoverSuspension : MonoBehaviour
    {
        #region public fields
        [Tooltip("Distance over the ground")]
        public float hoverDistance = 2.0f;
        [Tooltip("Spring stiffness. Higher values will result in harder to compress spring behaviour.")]
        public float springStiffness = 3.0f;
        [Tooltip("Prevent the spring from oscilating indefinitely")]
        public float springDampen = 1.0f;

        [Tooltip("actual distance of the hover raycast, should be greater than maxHoverDistance")]
        public float rayLength;

        [Tooltip("Layers ignored by the hover spring")]
        public LayerMask layerMask;

        [Tooltip("Rigidbody affected by this hover suspension")]
        public Rigidbody affectedBody;

        [Tooltip("Whether to apply the suspension force at the rigidbody's root or the world position of the hover suspension")]
        public bool applyForceToRoot = false;

        // should we use a secondary prediction ray based on current velocity?
        [Tooltip("Use prediction raycasts along our current velocity vector")]
        public bool usePrediction = true;
        [Tooltip("How far into the future should we predict?"), Range(0.01f, 3)]
        public float predictionTime = 0.2f;
        #endregion

        #region public properties

        public Vector3 groundNormal { get { return _groundNormal; } }
        public bool isGrounded { get { return _isGrounded; } }

        #endregion


        #region public functions

        #endregion

        #region private

        private float _prevSpringDelta;
        private Vector3 _groundNormal = Vector3.up;
        private bool _isGrounded;


        // if we ever want to change it
        private Vector3 GetSpringDirection()
        {
            return -transform.up;
            // While in the air we use our object normal
            // when grounded we use the last known ground normal
            if (!isGrounded)
                return -transform.up;
            else
                return -_groundNormal;
        }

        bool SpringRaycast(Vector3 direction, out float distance, out Vector3 normal)
        {
            distance = 0.0f;
            normal = Vector3.up;

            RaycastHit hitInfo;
            if (!Physics.Raycast(transform.position, direction, out hitInfo, rayLength, layerMask))
                return false;

            distance = hitInfo.distance;
            normal = hitInfo.normal;

            Debug.DrawLine(transform.position, hitInfo.point, Color.red);

            // secondary raycast in case we want to use prediction
            if (usePrediction)
            {
                Vector3 predictionOrigin = transform.position;
                Vector3 projectedVelocity = Vector3.ProjectOnPlane(affectedBody.velocity, normal);
                predictionOrigin += projectedVelocity * predictionTime;

                if (Physics.Raycast(predictionOrigin, direction, out hitInfo, rayLength, layerMask))
                {
                    normal += hitInfo.normal;
                    distance += hitInfo.distance;
                    normal /= 2.0f;
                    distance /= 2.0f;
                    Debug.DrawLine(predictionOrigin, hitInfo.point, Color.red);
                }

            }
            normal.Normalize();
            return true;
        }

        private void FixedUpdate()
        {
            Vector3 prevNormal = Vector3.up;

            _isGrounded = false;

            Vector3 springDirection = GetSpringDirection();

            // Do downward raycast
            RaycastHit hitInfo;
            _isGrounded = Physics.Raycast(transform.position, springDirection, out hitInfo, rayLength, layerMask);


            if (!_isGrounded)
                return;


            float hitPointDelta = (hitInfo.point - transform.position).magnitude;
            float springDelta = hoverDistance - hitPointDelta;
            float springVelocity = (_prevSpringDelta - springDelta) / Time.deltaTime;
            
            Vector3 hoverForce = hitInfo.normal * CalcSpringForce(springDelta, springVelocity);

            //if (hitPointDelta > hoverDistance)
                //hoverForce = Vector3.zero;

            _prevSpringDelta = springDelta;
            Debug.Log(hoverForce);
            if (applyForceToRoot)
                affectedBody.AddForce(hoverForce, ForceMode.Force);
            else
                affectedBody.AddForceAtPosition(hoverForce, transform.position, ForceMode.Force);


            _isGrounded = true;
            _groundNormal = hitInfo.normal;
        }

        private float CalcSpringForce(float delta, float velocity)
        {
            float springRatio = delta / hoverDistance;
            return (springRatio * springStiffness - springDampen * velocity);
        }
        #endregion
    }

}