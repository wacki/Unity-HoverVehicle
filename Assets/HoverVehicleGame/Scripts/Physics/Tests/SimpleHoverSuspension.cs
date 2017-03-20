using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHoverSuspension : MonoBehaviour {

    public float smoothTime = 0.2f;
    public float hoverDistance = 2.0f;

    public float rayLength = 5.0f;

    public LayerMask layerMask;

    private float _smoothVelocity;

    public Vector3 groundNormal;
    public bool isGrounded;

    // Update is called once per frame
    void Update () {

        RaycastHit hitInfo;
        isGrounded = Physics.Raycast(transform.position, -transform.up, out hitInfo, rayLength, layerMask);
        if (!isGrounded)
            return;
        groundNormal = hitInfo.normal;

        var newHeight = Mathf.SmoothDamp(hitInfo.distance, hoverDistance, ref _smoothVelocity, smoothTime);

        transform.position = hitInfo.point + hitInfo.normal * newHeight;
        
	}

    void FixedUpdate()
    {
        //var rb = GetComponent<Rigidbody>();
        //var vel = rb.velocity;
        //var impulse = Vector3.Project(vel, groundNormal);
        //rb.AddForce(impulse, ForceMode.Impulse);

    }
}
