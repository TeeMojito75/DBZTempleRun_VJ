using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    public float maxSpeed;
    private Vector3 targetPosition;
    public float forwardSpeed;

    private int desiredLane = 1; 
    public float laneDistance = 4; 
    public float laneChangeSpeed = 10f;

    public float jumpForce;
    public float gravity;

    public Animator animator;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!PlayerManager.isGameStarted)
        {
            return;
        }

        animator.SetBool("isGameStarted", true);

        //Increment de la velocitat
        if (forwardSpeed < maxSpeed)
            forwardSpeed += 0.25f * Time.deltaTime;
        //Moviment frontal
        direction.z = forwardSpeed;

        animator.SetBool("isGrounded", controller.isGrounded);
        //Controlador salts
        if (controller.isGrounded) //controla que nom�s botem desde enterra
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

        //Controlador lliscaments
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            StartCoroutine(Slide());
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

        // Calcula la seg�ent posici� del personatge
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

        // Mou el personatge a la posici� que s'ha calculat anteriorment
        targetPosition = Vector3.Lerp(transform.position, new Vector3(newTargetPosition.x, transform.position.y, transform.position.z), laneChangeSpeed * Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (!PlayerManager.isGameStarted)
        {
            return;
        }

        // Calcula la posición objetivo del carril
        Vector3 targetLanePosition = new Vector3(targetPosition.x, transform.position.y, transform.position.z);

        // Interpola suavemente hacia la posición objetivo del carril
        float interpolationFactor = laneChangeSpeed * Time.fixedDeltaTime;
        float newX = Mathf.Lerp(transform.position.x, targetLanePosition.x, interpolationFactor);

        // Crea el vector de movimiento
        Vector3 move = new Vector3(newX - transform.position.x, direction.y * Time.fixedDeltaTime, direction.z * Time.fixedDeltaTime);

        // Aplica el movimiento lateral y hacia adelante
        controller.Move(move);

        // Aplica el movimiento vertical después de mover lateralmente y hacia adelante
        controller.Move(new Vector3(0, direction.y * Time.fixedDeltaTime, 0));
    }

    private void Jump()
    {
        direction.y = jumpForce;
    }
    private IEnumerator Slide()
    {
        animator.SetBool("isSliding", true);
        controller.center = new Vector3(0, -1.35f, 0);
        controller.height = 1f;
        yield return new WaitForSeconds(1.3f);
        controller.center = new Vector3(0, -1.1f, 0);
        controller.height = 2f;
        animator.SetBool("isSliding", false);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.transform.tag == "Obstacle")
        {
            PlayerManager.gameOver = true;
            FindObjectOfType<AudioManager>().Play("GameOver");
        }
    }
}
