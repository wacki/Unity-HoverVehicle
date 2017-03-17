using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    public class NewHoverSuspension : MonoBehaviour
    {
        public enum UpDirType {
            SurfaceNormal,
            ObjectUp,
            HeightProbe
        }

        public UpDirType springDirection;
        
        // which rigidbody is affected by this spring
        public Rigidbody affectedBody;
        // should we use a secondary prediction ray based on current velocity?
        public bool usePrediction = true;
        [Range(1, 60)]
        public int predictionFrames = 1;

        // maximum distance from floor allowed by this spring
        public float maxHoverDistance;
        // minimum distance from floor allowed by this spring
        public float minHoverDistance;

        // ray length to use
        public float rayLength;

        // which layers to ignore with this raycast?
        public LayerMask layerMask;
        

        // current up dir to use for the spring
        private Vector3 _upDir;
        private bool _isGrounded = false;

        // keep track of previous spring delta for dampening
        private float _prevSpringDelta;

        public float springStiffness;
        public float springDampen;
        
        // internal spring values
        private float _hoverSpringLength;
        private float _hoverSpringHalfLength;

        // internal spring variables
        private float _springRatio;

        private KeepUprightConstraint _uprightConstraint;

        void Awake()
        {
            _hoverSpringLength = maxHoverDistance - minHoverDistance;
            _hoverSpringHalfLength = 0.5f * _hoverSpringLength;

            _uprightConstraint = GetComponent<KeepUprightConstraint>();
        }

        void Update()
        {
            // update spring (this is what we will actually use)
            UpdateSpring();

            // apply a random force to see if the cube can keep itself on track
            ApplyTestForces();
        }


        void GroundCheck()
        {



        }


        private Vector3 GetSpringDirection()
        {
            switch(springDirection)
            {
                case UpDirType.SurfaceNormal:
                    // use last known surface normal
                    if (_isGrounded)
                        return -_upDir;
                    // if we weren't grounded use local down direction
                    else
                        return -transform.up;
                    
                case UpDirType.ObjectUp:
                    return -transform.up;
                case UpDirType.HeightProbe:
                    // todo: check height probe setup for normal
                    var heightProbe = GetComponent<HeightProbe>();
                    if (heightProbe.isGrounded)
                    {
                        heightProbe.useCustomProbeDirection = true;
                        heightProbe.propeDirection = -heightProbe.currentNormal;
                        return -heightProbe.currentNormal;
                    }
                    else
                        heightProbe.useCustomProbeDirection = false;
                    break;
            }

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
            if(usePrediction)
            {
                Vector3 predictionOrigin = transform.position;
                predictionOrigin += affectedBody.velocity * Time.deltaTime * predictionFrames; 

                if(Physics.Raycast(predictionOrigin, direction, out hitInfo, rayLength, layerMask))
                {
                    normal += hitInfo.normal;
                    distance += hitInfo.distance;
                    normal /= 2.0f;
                    distance /= 2.0f;
                    Debug.DrawLine(predictionOrigin, hitInfo.point, Color.red);
                }

            }

            return true;
        }


        void UpdateSpring()
        {
            // get spring direction
            Vector3 springDirection = GetSpringDirection();

            // calculate current ground normal and distance to ground
            float impactDistance;
            _isGrounded = SpringRaycast(springDirection, out impactDistance, out _upDir);


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
            var springVelocity = (_prevSpringDelta - springDelta) / Time.deltaTime;
            _prevSpringDelta = springDelta;


            _springRatio = springDelta / _hoverSpringHalfLength;


            // snap back to min and max point if we're outside of the range
            if (_springRatio > 1.0f)
                transform.position = hitPoint - springDirection * minHoverDistance;
            if (_springRatio < -1.0f)
                transform.position = hitPoint - springDirection * maxHoverDistance;

            // calculate the actual spring force required
            float k = springStiffness * affectedBody.mass * Physics.gravity.magnitude;
            float b = springDampen * affectedBody.mass * Physics.gravity.magnitude;

            _springRatio = Mathf.Clamp(_springRatio, -1.0f, 1.0f);

            float springForce = (_springRatio * k - b * springVelocity);


            var hoverForce = _upDir * springForce;
            affectedBody.AddForce(hoverForce);

            _uprightConstraint.goalUpDir = -springDirection;
        }

        private Vector3 _randomForce;
        void ApplyTestForces()
        {
            Vector3 randomForce = new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));

            randomForce = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            //randomForce = transform.rotation* randomForce;
            randomForce = Vector3.ProjectOnPlane(randomForce, _upDir);
            randomForce.Normalize();
            
            affectedBody.AddForce(randomForce * 70, ForceMode.Acceleration);
            Debug.DrawLine(transform.position, transform.position + randomForce, Color.magenta);
        }

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

    }

}