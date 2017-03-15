using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HoverRacingGame
{
    public class HoverSuspension : MonoBehaviour
    {
        public float hoverDistance = 4.0f;
        public float springStiffness;
        public float springDampen;

        [Tooltip("Rigidbody affected by this hover suspension")]
        public Rigidbody affectedBody;
        [Tooltip("Whether to apply the suspension force at the rigidbody's root or the world position of the hover suspension")]
        public bool applyForceToRoot = false;


        public bool usePredictionRay = true;
        public float predictionFrameCount = 2f;
        
        public Vector3 groundNormal { get { return _groundNormal; } }
        public bool isGrounded { get { return _isGrounded; } }


        private float _prevSpringDelta;
        private Vector3 _groundNormal = Vector3.up;
        private bool _isGrounded;
        
        private void Update()
        {
            Vector3 prevNormal = Vector3.up;

            _isGrounded = false;

            // Do downward raycast
            RaycastHit hitInfo;
            if (!Physics.Raycast(transform.position, -transform.up, out hitInfo, hoverDistance))
                return;
            
            float hitPointDelta = (hitInfo.point - transform.position).magnitude;
            float springDelta = hoverDistance - hitPointDelta;
            float springVelocity = (_prevSpringDelta - springDelta) / Time.deltaTime;
            
            Vector3 hoverForce = hitInfo.normal * CalcSpringForce(springDelta, springVelocity);

            //if (hitPointDelta > hoverDistance)
                //hoverForce = Vector3.zero;

            _prevSpringDelta = springDelta;

            if (applyForceToRoot)
                affectedBody.AddForce(hoverForce);
            else
                affectedBody.AddForceAtPosition(hoverForce, transform.position);


            _isGrounded = true;
            _groundNormal = hitInfo.normal;
        }

        private float CalcSpringForce(float delta, float velocity)
        {
            float springRatio = delta / hoverDistance;
            return (springRatio * springStiffness - springDampen * velocity);
        }
    }

}