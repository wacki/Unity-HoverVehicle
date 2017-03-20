using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    public class HoverSuspension : MonoBehaviour
    {
        #region public fields
        // should we use a secondary prediction ray based on current velocity?
        [Tooltip("Use prediction raycasts along our current velocity vector")]
        public bool usePrediction = true;
        [Tooltip("How far into the future should we predict?"), Range(0.01f, 3)]
        public float predictionTime = 0.2f;

        // The vehicle will snap to these min and max values!
        [Tooltip("maximum distance from floor allowed by this spring")]
        public float maxHoverDistance;
        [Tooltip("minimum distance from floor allowed by this spring")]
        public float minHoverDistance;

        [Tooltip("actual distance of the hover raycast, should be greater than maxHoverDistance")]
        public float rayLength;

        [Tooltip("Layers ignored by the hover spring")]
        public LayerMask layerMask;

        [Tooltip("Rigidbody affected by this suspension")]
        public Rigidbody affectedBody;

        [Tooltip("Rigidbody affected by this suspension")]
        public bool applyForceToRoot = true;

        [Tooltip("Stiffness of the suspension spring")]
        public float springStiffness = 3.0f;
        [Tooltip("Dampening factor of the suspension spring")]
        public float springDampen = 1.0f;        

        #endregion

        #region public properties

        public Vector3 groundNormal { get { return _groundNormal; } }
        public bool isGrounded { get { return _isGrounded; } }

        #endregion


        #region public functions

        #endregion

        #region private

        // internal spring values
        private float _hoverSpringLength;
        private float _hoverSpringHalfLength;
        private float _prevSpringDelta;

        // internal spring variables
        private float _springRatio;

        private Vector3 _groundNormal;
        private bool _isGrounded;

        private void Awake()
        {
            _hoverSpringLength = maxHoverDistance - minHoverDistance;
            _hoverSpringHalfLength = 0.5f * _hoverSpringLength;
        }

        private void FixedUpdate()
        {
            // update spring (this is what we will actually use)
            UpdateSpring();
        }

        // if we ever want to change it
        private Vector3 GetSpringDirection()
        {
            return -transform.up;
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


        void UpdateSpring()
        {
            // get spring direction
            Vector3 springDirection = GetSpringDirection();

            // calculate current ground normal and distance to ground
            float impactDistance;
            _isGrounded = SpringRaycast(springDirection, out impactDistance, out _groundNormal);


            // early out if we're not grounded
            if (!_isGrounded)
                return;


            Vector3 hitPoint = transform.position + springDirection * impactDistance;

            // calculate world space min and max spring points
            var minHoverPoint = transform.position + springDirection * minHoverDistance;
            var maxHoverPoint = minHoverPoint + springDirection * _hoverSpringLength;
            var springEquilibriumPoint = minHoverPoint + 0.5f * springDirection * _hoverSpringLength;

            // current delta from spring equilibrium point
            var springDelta = (minHoverDistance + _hoverSpringHalfLength) - impactDistance;
            var test = (_prevSpringDelta - springDelta);
            test = Mathf.Clamp(test, 0.0f, 1.0f);
            var springVelocity = (_prevSpringDelta - springDelta) / Time.deltaTime;
            _prevSpringDelta = springDelta;


            _springRatio = springDelta / _hoverSpringHalfLength;


            // snap back to min and max point if we're outside of the range
            if (_springRatio > 1.0f)
                transform.position = hitPoint - springDirection * minHoverDistance;
            if (_springRatio < -1.0f)
                transform.position = hitPoint - springDirection * maxHoverDistance;

            // calculate the actual spring force required
            float k = springStiffness * affectedBody.mass;
            float b = springDampen * affectedBody.mass;

            _springRatio = Mathf.Clamp(_springRatio, -1.0f, 1.0f);

            float springForce = (_springRatio * k - b * springVelocity);


            var hoverForce = -springDirection * springForce;

            if(applyForceToRoot)
                affectedBody.AddForce(hoverForce);
            else
                affectedBody.AddForceAtPosition(hoverForce, transform.position);            
        }



        #endregion

        #region editor

#if UNITY_EDITOR

        void OnDrawGizmos()
        {
            Vector3 springDirection = GetSpringDirection();
            //if (!_isGrounded)
            // return;

            var minHoverPoint = transform.position + springDirection * minHoverDistance;
            var maxHoverPoint = minHoverPoint + springDirection * _hoverSpringLength;
            var springEquilibriumPoint = minHoverPoint + 0.5f * springDirection * _hoverSpringLength;

            // debug out
            Debug.DrawLine(transform.position, minHoverPoint, Color.gray);
            Debug.DrawLine(maxHoverPoint, maxHoverPoint + springDirection * (rayLength - maxHoverDistance), Color.gray);

            Color midRayColor = Color.Lerp(Color.green, Color.red, Mathf.Abs(_springRatio));
            Debug.DrawLine(minHoverPoint, maxHoverPoint, midRayColor);

            Gizmos.color = midRayColor;
            Gizmos.DrawSphere(minHoverPoint, 0.1f);
            Gizmos.DrawSphere(maxHoverPoint, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(springEquilibriumPoint, new Vector3(0.2f, 0.01f, 0.2f));



        }

#endif
        #endregion

    }

}