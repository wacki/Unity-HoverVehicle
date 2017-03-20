using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    public class FinalHoverRacerTest2 : HoverVehicleBase
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
        

        private void Start()
        {
        }

        protected override void Update()
        {
            base.Update();
            
        }

        protected override Vector3 GetGroundNormal()
        {
            return transform.up;
        }

        protected override bool IsGrounded()
        {
            return true;
        }
    }

}