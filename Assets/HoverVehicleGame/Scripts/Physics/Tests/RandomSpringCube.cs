using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{

    /// <summary>
    /// I added this script to have a test case for a hover object that can keep itself on a track
    /// no matter the surface. This cube can be placed on any track and it will proppel itself 
    /// randomly. It should be able to stay on the track. Once we values that work
    /// for this cube we can transfer them on an actual hover ship.
    /// </summary>
    public class RandomSpringCube : MonoBehaviour
    {
        enum HoverState
        {
            OnTrack,
            Airborne
        }

        /// <summary>
        /// The cube will try to stay at the mid point between min and max
        /// </summary>
        public float maxHoverDistance;
        public float minHoverDistance;


        private HoverState _state = HoverState.OnTrack;

        private Vector3 _upDir;
        private bool _isGrounded = false;

        void Awake()
        {

        }
        
        void Update()
        {
            _upDir = Vector3.up;
            _isGrounded = false;

            RaycastHit hitInfo;
            if (!Physics.Raycast(transform.position, -transform.up, out hitInfo, maxHoverDistance))
                return;
            
            _upDir = hitInfo.normal;
            _isGrounded = true;

            transform.position = hitInfo.point + _upDir * minHoverDistance;
        }

        /// <summary>
        /// Make sure we're facing towards the current up direction
        /// </summary>
        void OrientToUpDir()
        {
            // for now we just force the exact up direction
            transform.rotation = Quaternion.FromToRotation(_upDir, transform.up);
        }
    }

}