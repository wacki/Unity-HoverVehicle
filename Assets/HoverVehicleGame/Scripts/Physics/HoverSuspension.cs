using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HoverRacingGame
{
    [RequireComponent(typeof(Rigidbody))]
    public class HoverSuspension : MonoBehaviour
    {
        public float hoverDistance = 4.0f;
        public float springStiffness;
        public float springDampen;

        public float tempK = 3f;
        public float tempD = 1f;

        public float tempPredictionFrames = 2f;


        private Vector3 _goalUpDir;
        private float _prevSpringDelta;

        private float _prevAngle;


        // testwise setter for goal up vector just to see if it helps
        public Vector3 goalUpVector { set { _goalUpDir = value; } }

        private Rigidbody _rb;

        private void Awake()
        {
            _goalUpDir = new Vector3(0, 1, 0);
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            UpdateUprightConstraint();


            Vector3 hitPosition = Vector3.zero;
            Vector3 hitNormal = Vector3.up;
            int hitCount = 0;

            Vector3 prevNormal = Vector3.up;

            // Do downward raycast
            RaycastHit hitInfo;
            if (Physics.Raycast(transform.position, -transform.up, out hitInfo, hoverDistance * 3))
            {
                hitPosition = hitInfo.point;
                hitNormal = hitInfo.normal;
                hitCount++;
                Debug.DrawLine(transform.position, hitPosition, Color.green);
                prevNormal = hitNormal;
            }
            else
                Debug.DrawLine(transform.position, transform.position - transform.up * hoverDistance * 3, Color.red);

            // prediction ray
            var predictedPosition = transform.position + _rb.velocity * tempPredictionFrames * Time.deltaTime;
            var predictedUp = Quaternion.Euler(_rb.angularVelocity * tempPredictionFrames * Time.deltaTime) * transform.up;

            if (Physics.Raycast(predictedPosition, -predictedUp, out hitInfo, hoverDistance * 3))
            {
                hitCount++;
                hitNormal += hitInfo.normal;
                hitNormal /= hitCount;
                hitNormal.Normalize();
                Debug.DrawLine(predictedPosition, hitInfo.point, Color.green);
            }
            else
            {
                Debug.DrawLine(predictedPosition, predictedPosition - predictedUp * hoverDistance * 3, Color.red);
            }

            if (hitCount < 1)
                return;


            Debug.DrawLine(transform.position, transform.position + hitNormal, Color.blue);
            Debug.DrawLine(transform.position, transform.position + hitInfo.normal, Color.cyan);
            Debug.DrawLine(transform.position, transform.position + prevNormal, Color.magenta);

            _goalUpDir = hitNormal;

            float hitPointDelta = (hitPosition - transform.position).magnitude;
            float springDelta = hoverDistance - hitPointDelta;
            float springVelocity = (_prevSpringDelta - springDelta) / Time.deltaTime;


            Vector3 hoverForce = _goalUpDir * CalcSpringForce(springDelta, springVelocity);

            if (hitPointDelta > hoverDistance)
                hoverForce = Vector3.zero;

            _prevSpringDelta = springDelta;

            _rb.AddForce(hoverForce);

        }


        private void UpdateUprightConstraint()
        {
            var up = transform.up;

            Quaternion deltaRot = Quaternion.FromToRotation(_goalUpDir, up);
            float angle;
            Vector3 axis;
            deltaRot.ToAngleAxis(out angle, out axis);

            Debug.Log("Angle offset " + angle + " " + deltaRot.eulerAngles);


            float restDelta = angle;

            float rotSpringVelocity = (_prevAngle - restDelta) / Time.deltaTime;

            Vector3 eulerDelta = deltaRot.eulerAngles;
            // fuck unity for not using negative angles

            eulerDelta.x = (eulerDelta.x > 180) ? eulerDelta.x - 360 : eulerDelta.x;
            eulerDelta.y = (eulerDelta.y > 180) ? eulerDelta.y - 360 : eulerDelta.y;
            eulerDelta.z = (eulerDelta.z > 180) ? eulerDelta.z - 360 : eulerDelta.z;

            Vector3 torque = -tempK * eulerDelta - tempD * _rb.angularVelocity;
            //torque.y = 0.0f;
            _rb.AddTorque(torque);

            _prevAngle = restDelta;
        }

        private float CalcSpringForce(float delta, float velocity)
        {
            float springRatio = delta / hoverDistance;
            return (springRatio * springStiffness - springDampen * velocity);
        }



    }

}