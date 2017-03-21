using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    public class FinalHoverRacerTest : HoverVehicleBase
    {
        //###########################################
        //######## BEGIN TEST CODE VARIABLES ########
        // different types of up vectors to switch between
        public enum UpDirType
        {
            SurfaceNormal,
            ObjectUp,
            HeightProbe
        }

        public UpDirType springDirection;

        public bool limitMaxSpeed = false;

        //######## END TEST CODE VARIABLES ##########
        //###########################################

        public HoverSuspension hoverSuspension;

        private KeepUprightConstraint _uprightConstraint;

        public float maxSpeedKMH = 300.0f;

        private float _curSpeed;

        private void Start()
        {
            _uprightConstraint = GetComponent<KeepUprightConstraint>();
        }

        protected override void Update()
        {
            base.Update();

            // more test code
            var keepUpright = GetComponent<KeepUprightConstraint>();
            if (keepUpright != null)
                keepUpright.goalUpDir = GetGroundNormal();


            var simpleKeepUpright = GetComponent<SimpleKeepUprightConstraint>();
            if (simpleKeepUpright != null)
                simpleKeepUpright.goalUpDir = GetGroundNormal();

            var vel = GetComponent<Rigidbody>().velocity;

            _curSpeed = MStoKMH(vel.magnitude);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!limitMaxSpeed)
                return;

            if (_curSpeed > maxSpeedKMH)
            {
                var vel = _rb.velocity;
                var goalSpeed = KMHtoMS(maxSpeedKMH);


                var velocityDelta = vel.magnitude - goalSpeed;


                _rb.AddForce(vel.normalized * (goalSpeed - vel.magnitude), ForceMode.Acceleration);
                Debug.Log("Brake Impulse " + (vel.normalized * (goalSpeed - vel.magnitude)));
            }
        }


        protected override void Move(float moveValue)
        {
            if (_curSpeed > maxSpeedKMH)
                return;

            Vector3 tangentForward = Vector3.ProjectOnPlane(transform.forward, GetGroundNormal());
            tangentForward.Normalize();

            Vector3 force = tangentForward * thrusterForce * moveValue;
            _rb.AddForce(force);
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

        protected override Vector3 GetGroundNormal()
        {
            return hoverSuspension.groundNormal;
        }

        protected override bool IsGrounded()
        {
            return hoverSuspension.isGrounded;
        }
    }

}