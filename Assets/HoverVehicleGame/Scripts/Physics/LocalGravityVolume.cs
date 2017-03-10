using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    public class LocalGravityVolume : MonoBehaviour
    {
        public float gravityAcceleration = 9.81f;
        private List<Rigidbody> affectedRBs = new List<Rigidbody>();

        private void OnTriggerEnter(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null)
                return;

            // todo: make sure we remove rbs! Sometimes on trigger exit isn't called properly
            affectedRBs.Add(rb);
        }


        private void OnTriggerExit(Collider other)
        {
            var rb = other.attachedRigidbody;
            if (rb == null)
                return;

            affectedRBs.Remove(rb);
        }


        private void Update()
        {
            for (int i = 0; i < affectedRBs.Count; i++)
            {
                var rb = affectedRBs[i];

                // for now we just pretend we're a straight cylindrical force. 
                // So we zero the X component and use the remaining delta vector
                // as a gravity direction
                var rbPos = rb.transform.position;
                var gravityClosestPoint = transform.position;
                rbPos.x = 0;
                gravityClosestPoint.x = 0;

                var offset = gravityClosestPoint - rbPos;
                rb.AddForce(offset.normalized * gravityAcceleration, ForceMode.Acceleration);

                var hs = rb.GetComponent<HoverSuspension>();
                //if (hs != null)
                  //  hs.goalUpVector = -offset.normalized;


            }
        }

    }

}