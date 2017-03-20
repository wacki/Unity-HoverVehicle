using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    /// <summary>
    /// Simple alternative to the physics based keep upright constraint
    /// </summary>
    public class SimpleKeepUprightConstraint : MonoBehaviour
    {
        public Vector3 goalUpDir;
        public float smoothTime;

        private Vector3 _velocity;

        // Update is called once per frame
        void Update()
        {
            transform.up = Vector3.SmoothDamp(transform.up, goalUpDir, ref _velocity, smoothTime);
        }
    }
}
