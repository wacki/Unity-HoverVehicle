using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace HoverRacingGame
{

    /// <summary>
    /// I added this script to have a test case for a hover object that can keep itself on a track
    /// no matter the surface. This cube can be placed on any track and it will proppel itself 
    /// randomly. It should be able to stay on the track. Once we values that work
    /// for this cube we can transfer them on an actual hover ship.
    /// 
    /// todo: clean this stuff up, ew..
    /// 
    /// 
    /// 
    /// Stuff to try:
    /// 
    /// 1. Align hover spring ray with previous normal. If no normal was hit then try to use the object's down direction
    /// 2. Always use object's down direction
    /// 3. 
    /// </summary>
    public class RandomSpringCube : MonoBehaviour
    {
        enum HoverState
        {
            OnTrack,
            Airborne
        }

        /// <summary>
        /// The cube will try to stay at the mid point between min and max
        /// </summary>
        public float maxHoverDistance;
        public float minHoverDistance;

        public float rayLength;

        public LayerMask layerMask;

        public Rigidbody affectedBody;

        private HoverState _state = HoverState.OnTrack;

        private Vector3 _upDir;
        private bool _isGrounded = false;

        private float _prevSpringDelta;

        public float springStiffness;
        public float springDampen;

        private float _springForce;
        private float _springDampen;


        // internal spring values
        private float _hoverSpringLength;
        private float _hoverSpringHalfLength;

        // internal spring variables
        private float _springRatio;

        private KeepUprightConstraintOLD _uprightConstraint;

        void Awake()
        {
            _springForce = springStiffness * affectedBody.mass * Physics.gravity.magnitude;
            _springDampen = springDampen * affectedBody.mass;

            _hoverSpringLength = maxHoverDistance - minHoverDistance;
            _hoverSpringHalfLength = 0.5f * _hoverSpringLength;

            _uprightConstraint = GetComponent<KeepUprightConstraintOLD>();
        }
        
        void Update()
        {
            // update spring (this is what we will actually use)
            UpdateSpring();

            // apply a random force to see if the cube can keep itself on track
            //ApplyTestForces();
        }
        

        void UpdateSpring()
        {
            _upDir = Vector3.up;
            _isGrounded = false;

            RaycastHit hitInfo;
            if (!Physics.Raycast(transform.position, -transform.up, out hitInfo, rayLength, layerMask))
                return;

            // ignore if specific script is attached
            if (hitInfo.collider.GetComponent<IgnoreHoverCube>() != null)
                return;

            // update is grounded status and up dir
            _upDir = hitInfo.normal;

            // todo: only update is grounded if our current state allows it
            _isGrounded = true;


            // calculate world space min and max spring points
            var minHoverPoint = transform.position - transform.up * minHoverDistance;
            var maxHoverPoint = minHoverPoint - transform.up * _hoverSpringLength;
            var springEquilibriumPoint = minHoverPoint - 0.5f * transform.up * _hoverSpringLength;

            // current delta from spring equilibrium point
            var hitPointDelta = (hitInfo.point - transform.position).magnitude;
            var springDelta = (minHoverDistance + _hoverSpringHalfLength) - hitPointDelta;


            //transform.position = hitInfo.point + _upDir * minHoverDistance;
            var springVelocity = (_prevSpringDelta - springDelta) / Time.deltaTime;

            _prevSpringDelta = springDelta;


            _springRatio = springDelta / _hoverSpringHalfLength;


            // snap back to min and max point if we're outside of the range
            if (_springRatio > 1.0f)
                transform.position = hitInfo.point + transform.up * minHoverDistance;
            if (_springRatio < -1.0f)
                transform.position = hitInfo.point + transform.up * maxHoverDistance;



            float springForce = (_springRatio * _springForce - _springDampen * springVelocity);


            var hoverForce = _upDir * springForce;
            affectedBody.AddForce(hoverForce);

            _uprightConstraint.goalUpDir = _upDir;
        }

        private Vector3 _randomForce;
        void ApplyTestForces()
        {
            Vector3 randomForce = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
            randomForce = Vector3.ProjectOnPlane(randomForce, _upDir);
            randomForce.Normalize();

            affectedBody.AddForce(randomForce * 70, ForceMode.Acceleration);
            Debug.DrawLine(transform.position, transform.position + randomForce, Color.magenta);
        }

        void OnDrawGizmos()
        {
            //if (!_isGrounded)
               // return;

            var minHoverPoint = transform.position - transform.up * minHoverDistance;
            var maxHoverPoint = minHoverPoint - transform.up * _hoverSpringLength;
            var springEquilibriumPoint = minHoverPoint - 0.5f * transform.up * _hoverSpringLength;

            // debug out
            Debug.DrawLine(transform.position, minHoverPoint, Color.gray);
            Debug.DrawLine(maxHoverPoint, maxHoverPoint - transform.up * (rayLength - maxHoverDistance), Color.gray);

            Color midRayColor = Color.Lerp(Color.green, Color.red, Mathf.Abs(_springRatio));
            Debug.DrawLine(minHoverPoint, maxHoverPoint, midRayColor);

            Gizmos.color = midRayColor;
            Gizmos.DrawSphere(minHoverPoint, 0.1f);
            Gizmos.DrawSphere(maxHoverPoint, 0.1f);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(springEquilibriumPoint, new Vector3(0.2f, 0.01f, 0.2f));



        }
    }

}