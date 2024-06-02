using UnityEngine;
using System.Collections;

public class PlayerMove : MonoBehaviour
{
    private CharacterController controller;
    private Vector3 direction;
    public float maxSpeed;
    private Vector3 targetPosition;
    public static float forwardSpeed;

    private int desiredLane = 1;
    public float laneDistance = 3;
    public float laneChangeSpeed = 50f;

    public float jumpForce;
    public float gravity;
    public static bool hurt = false;
    private bool canCollide = true;
    public float collisionCooldown = 0.5f;

    public static bool godMode = false;

    public Animator animator;

    // Distancia para detectar obstáculos
    public float obstacleDetectionDistance = 4.0f; // Aumenta la distancia para detectar antes los obstáculos

    public float maxObstacleDetectionDistance = 10.0f;
    // Tag de los obstáculos
    public string obstacleTag = "Obstacle";

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        hurt = false;
        godMode = false;
        canCollide = true;
        forwardSpeed = 15f;
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

    if (obstacleDetectionDistance < maxObstacleDetectionDistance)
        obstacleDetectionDistance += 0.1f * Time.deltaTime;

    animator.SetBool("isGrounded", controller.isGrounded);

    //Controlador salts
    if (controller.isGrounded) //controla que nomás botem desde enterra
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

    if (Input.GetKeyUp(KeyCode.G))
    {
        godMode = !godMode;
    }

    if (Input.GetKeyUp(KeyCode.P))
    {
        PlayerManager.paused = !PlayerManager.paused;
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

        // Detectar obstáculos y saltar automáticamente
        DetectAndJumpOverObstacles();
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
        if (hit.transform.CompareTag(obstacleTag) || hit.transform.CompareTag("ObstacleSlide"))
        {
            if(!godMode) {
                PlayerManager.gameOver = true;
                FindObjectOfType<AudioManager>().Stop("MainTheme");
                FindObjectOfType<AudioManager>().Play("GameOver");
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (canCollide)
        {
            if (other.CompareTag("ObstacleHurt"))
            {
                if (hurt == false)
                {
                    SetHurtTrue();
                    FindObjectOfType<AudioManager>().Play("Tropezar");
                }
                else
                {
                    if(!godMode) {
                        PlayerManager.gameOver = true;
                        FindObjectOfType<AudioManager>().Play("GameOver");
                        FindObjectOfType<AudioManager>().Stop("MainTheme");
                    }
                }
                StartCoroutine(CollisionCooldown());
            }
        }
        if(godMode) {
            if (other.CompareTag("HoleLeft") || other.CompareTag("HoleRight"))
        {
            desiredLane = 1;
        }
        }
    }

    private void DetectAndJumpOverObstacles()
{   
    if(!godMode) return; // Si el modo dios está desactivado, salir de la función

    // Definir el punto de inicio del Raycast en la posición de la hitbox del jugador
    Vector3 rayStart = controller.bounds.center;

    // Raycast principal
    RaycastHit hit;
    Debug.DrawRay(rayStart, -transform.forward * obstacleDetectionDistance, Color.red);

    if (Physics.Raycast(rayStart, -transform.forward, out hit, obstacleDetectionDistance))
    {
        if (hit.transform.CompareTag(obstacleTag))
        {
            if (controller.isGrounded)
            {
                Jump();
                return; // Salir de la función para evitar comprobar otras alturas
            }
        }
    }

    rayStart = controller.bounds.center + Vector3.up; // Ajusta la altura según sea necesario
    Debug.DrawRay(rayStart, -transform.forward * obstacleDetectionDistance, Color.blue);

    if (Physics.Raycast(rayStart, -transform.forward, out hit, obstacleDetectionDistance))
    {
        
        if (hit.transform.CompareTag("ObstacleSlide"))
        {
            if (controller.isGrounded)
            {
                StartCoroutine(Slide());
                return; // Salir de la función para evitar comprobar otras alturas
            }
        }
        OnTriggerEnter(hit.collider);
    }
}
    private IEnumerator CollisionCooldown()
    {
        canCollide = false;
        yield return new WaitForSeconds(collisionCooldown);
        canCollide = true;
    }
    public void SetHurtTrue()
    {
        hurt = true;
        StartCoroutine(ResetHurtAfterDelay(10f));
    }

    private IEnumerator ResetHurtAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hurt = false;
    }
}
