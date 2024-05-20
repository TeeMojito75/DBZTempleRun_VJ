using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    private Vector3 targetPosition;
    public float forwardSpeed;

    private int desiredLane = 1; 
    public float laneDistance = 4; 
    public float laneChangeSpeed = 10f;

    public float jumpForce;
    public float gravity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        //Moviment frontal
        direction.z = forwardSpeed;

        if (controller.isGrounded) //controla que només botem desde enterra
        {   
            if (Input.GetKeyUp(KeyCode.UpArrow))
            {
                Jump();
            }
        }
        else
        {
            direction.y += gravity * Time.deltaTime;

        }

        // Moviment lateral
        if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            desiredLane++;
            if (desiredLane > 2)
                desiredLane = 2;
        }

        if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            desiredLane--;
            if (desiredLane < 0)
                desiredLane = 0;
        }

        // Calcula la següent posició del personatge
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

        // Mou el personatge a la posició que s'ha calculat anteriorment
        targetPosition = Vector3.Lerp(transform.position, new Vector3(newTargetPosition.x, transform.position.y, transform.position.z), laneChangeSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        //Aplica els canvis de moviment al jugador
        Vector3 move = new Vector3(targetPosition.x - transform.position.x, direction.y, direction.z * Time.fixedDeltaTime);
        controller.Move(move);
    }

    private void Jump()
    {
        direction.y = jumpForce;
    }
}
