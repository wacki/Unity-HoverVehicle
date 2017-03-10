using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HoverRacingGame
{
    public class VehicleMotor : MonoBehaviour
    {
        public float thrusterForce = 10000;
        public float turnTorque = 1000;



        // todo: distinguish between brake force and throttle forward and backwards!
        void Update()
        {
            var v = Input.GetAxis("Vertical");
            var h = Input.GetAxis("Horizontal");
            var throttle = Input.GetAxis("Throttle");
            var brake = Input.GetAxis("Brake");

            var mainThrusterForce = throttle + brake;


            var rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * thrusterForce * mainThrusterForce);
            rb.AddTorque(transform.rotation * new Vector3(0, h * turnTorque, 0));

        }
    }

}