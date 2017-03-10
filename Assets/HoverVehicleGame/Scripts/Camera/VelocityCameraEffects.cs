using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityCameraEffects : MonoBehaviour {

    public float minVelocityFOV = 70.0f;
    public float maxVelocityFOV = 120.0f;

    public float minVelocity = 1.0f;
    public float maxVelocity = 20.0f;

    public Rigidbody targetRigidbody;
    
	void Update () {

        float curVelocity = targetRigidbody.velocity.magnitude;

        float factor = (curVelocity - minVelocity) / (maxVelocity - minVelocity);
        factor = Mathf.Clamp01(factor);

        float fov = minVelocityFOV + factor * (maxVelocityFOV - minVelocityFOV);

        GetComponent<Camera>().fieldOfView = fov;
	}
}
