using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    public class FinalHoverRacerTest1 : HoverVehicleBase
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

        //######## END TEST CODE VARIABLES ##########
        //###########################################

        public SimpleHoverSuspension hoverSuspension;

        private KeepUprightConstraint _uprightConstraint;

        private void Start()
        {
            _uprightConstraint = GetComponent<KeepUprightConstraint>();
        }

        protected void Update()
        {
            // more test code
            var keepUpright = GetComponent<KeepUprightConstraint>();
            if (keepUpright != null)
                keepUpright.goalUpDir = GetGroundNormal();


            var simpleKeepUpright = GetComponent<SimpleKeepUprightConstraint>();
            if (simpleKeepUpright != null)
                simpleKeepUpright.goalUpDir = GetGroundNormal();
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