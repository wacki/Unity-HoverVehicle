using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    public class FollowCamera : MonoBehaviour
    {
        public Vector3 offset = new Vector3(0, 4, -10);

        public GameObject target;

        private Vector3 _goalPosition;
        private Vector3 _smoothVelocity;

        void Update()
        {
            _goalPosition = target.transform.position + target.transform.rotation * offset;


            transform.position = Vector3.SmoothDamp(transform.position, _goalPosition, ref _smoothVelocity, 0.1f);
            transform.LookAt(target.transform);


        }
    }

}