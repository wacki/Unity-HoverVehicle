using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PencilTurnScript : MonoBehaviour
{


    public float turnAcceleration = 50.0f;

    public Transform frontTurnPoint;
    public Transform rearTurnPoint;

    [Range(1, 2000)]
    public float rearMod = 3.0f;

    void Update()
    {

        var turnValue = Input.GetAxis("Horizontal");
        var _rb = GetComponent<Rigidbody>();
        _rb.AddForceAtPosition(transform.forward * turnValue * turnAcceleration, frontTurnPoint.position, ForceMode.Acceleration);
        _rb.AddForceAtPosition(-transform.forward * turnValue * turnAcceleration * rearMod, rearTurnPoint.position, ForceMode.Acceleration);
    }
}
