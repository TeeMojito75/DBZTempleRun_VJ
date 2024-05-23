using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public Transform target;
    private Vector3 offset;
    public float smoothSpeed = 0.125f;

    void Start()
    {
        offset = transform.position - target.position;
    }

    void Update()
    {
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
