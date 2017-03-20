using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HoverRacingGame
{
    public class MultiRayHoverVehicle : HoverVehicleBase
    {
        [Header("MultiRayHoverVehicle Fields")]
        public HoverSuspensionOLD[] hoverSuspensions;

        private void Start()
        {
            for(int i = 0; i < hoverSuspensions.Length; i++)
            {
                hoverSuspensions[i].applyForceToRoot = false;
            }
        }

        /// <summary>
        /// todo: not sure if we should use a separate raycast to determine our normal instead
        /// of the averaged normal from all the suspensions. We should test that
        /// </summary>
        /// <returns></returns>
        protected override Vector3 GetGroundNormal()
        {
            Vector3 normal = Vector3.zero;
            int normalCount = 0;

            for (int i = 0; i < hoverSuspensions.Length; i++)
            {

                if (!hoverSuspensions[i].isGrounded)
                    continue;

                normal += hoverSuspensions[i].groundNormal;
                normalCount++;
            }

            if (normalCount == 0)
                return normal;

            return normal / normalCount;            
        }

        protected override bool IsGrounded()
        {
            for (int i = 0; i < hoverSuspensions.Length; i++)
            {
                if (hoverSuspensions[i].isGrounded)
                    return true;
            }

            return false;
        }
    }

}