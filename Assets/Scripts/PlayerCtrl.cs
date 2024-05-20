using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    private Vector3 targetPosition;
    public float forwardSpeed;

    private int desiredLane = 1; // 0: left, 1: middle, 2: right
    public float laneDistance = 4; // Distance between 2 lanes
    public float laneChangeSpeed = 10f; // Speed of lane change

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        targetPosition = transform.position;
    }

    void Update()
    {
        // Set forward direction
        direction.z = forwardSpeed;

        // Gather the inputs on which lane we should be
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            desiredLane++;
            if (desiredLane > 2)
                desiredLane = 2;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            desiredLane--;
            if (desiredLane < 0)
                desiredLane = 0;
        }

        // Calculate the target position based on the desired lane
        Vector3 newTargetPosition = transform.position.z * transform.forward + transform.position.y * transform.up;

        if (desiredLane == 0)
        {
            newTargetPosition += Vector3.left * laneDistance;
        }
        else if (desiredLane == 1)
        {
            newTargetPosition += Vector3.zero; // Middle lane, no offset needed
        }
        else if (desiredLane == 2)
        {
            newTargetPosition += Vector3.right * laneDistance;
        }

        // Smoothly move the character towards the target position in the x direction
        targetPosition = Vector3.Lerp(transform.position, new Vector3(newTargetPosition.x, transform.position.y, transform.position.z), laneChangeSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        // Combine forward movement with lateral movement
        Vector3 move = new Vector3(targetPosition.x - transform.position.x, 0, direction.z * Time.fixedDeltaTime);
        controller.Move(move);
    }
}
