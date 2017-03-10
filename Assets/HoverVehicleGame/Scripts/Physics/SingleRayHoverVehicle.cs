using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    [RequireComponent(typeof(KeepUprightConstraint))]
    public class SingleRayHoverVehicle : HoverVehicleBase
    {
        [Header("SingleRayHoverVehicle Fields")]
        public HoverSuspension hoverSuspension;

        private KeepUprightConstraint _uprightConstraint;

        private void Start()
        {
            _uprightConstraint = GetComponent<KeepUprightConstraint>();

            // apply hover force to root
            hoverSuspension.applyForceToRoot = true;
        }

        protected override void Update()
        {
            base.Update();

            _uprightConstraint.goalUpDir = hoverSuspension.groundNormal;
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