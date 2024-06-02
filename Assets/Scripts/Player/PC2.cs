using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TempleRun.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PC2 : MonoBehaviour
    {
        [SerializeField]
        private float initialPlayerSpeed = 15f;
        [SerializeField]
        private float horizontalSpeed = 15f;
        [SerializeField]
        private float maximumPlayerSpeed = 50f;
        [SerializeField]
        private float playerSpeedIncreaseRate = .25f;
        [SerializeField]
        private float jumpHeight = 3.0f;
        [SerializeField]
        private float initialGravityValue = -80f;
        [SerializeField]
        private LayerMask groundLayer;
        [SerializeField]
        private LayerMask turnLayer;
        [SerializeField]
        private float raycastHeightOffset = 1f; // Altura para el Raycast

        private float playerSpeed;
        private float currentGravity;
        private Vector3 movementDirection = Vector3.forward;
        private Vector3 playerVelocity;

        private PlayerInput playerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;
        private InputAction moveAction; // Acción de movimiento horizontal
        private bool isSliding = false;

        public CharacterController controller;
        public static bool hurt = false;
        private bool canCollide = true;
        public float collisionCooldown = 0.5f;

        public static bool godMode = false;
        public float obstacleDetectionDistance = 4.0f; 

        public float maxObstacleDetectionDistance = 10.0f;

        private bool canTurn = true; // Bandera para controlar el cooldown del giro
        public float turnCooldown = 0.5f; // Tiempo de cooldown para el giro
        
        private Vector3 originalDirection;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            controller = GetComponent<CharacterController>();
            turnAction = playerInput.actions["Turn"];
            jumpAction = playerInput.actions["Jump"];
            slideAction = playerInput.actions["Slide"];
            moveAction = playerInput.actions["Move"]; // Acción de movimiento horizontal
        }

        private void OnEnable()
        {
            turnAction.performed += PlayerTurn;
            slideAction.performed += PlayerSlide;
            jumpAction.performed += PlayerJump;
            moveAction.started += StartMoving; // Asignar el método de inicio de movimiento
            moveAction.canceled += StopMoving; // Asignar el método de detención de movimiento
        }

        private void OnDisable()
        {
            turnAction.performed -= PlayerTurn;
            slideAction.performed -= PlayerSlide;
            jumpAction.performed -= PlayerJump;
            moveAction.started -= StartMoving;
            moveAction.canceled -= StopMoving;
        }

        private void Start()
        {
            playerSpeed = initialPlayerSpeed;
            currentGravity = initialGravityValue;
            controller = GetComponent<CharacterController>();
            hurt = false;
            godMode = false;
            canCollide = true;

            // Asegurarse de que la dirección de movimiento esté inicializada a cero
            movementDirection = Vector3.zero;
        }

        private void PlayerTurn(InputAction.CallbackContext context)
        {
            if (!canTurn) return; // Si no se puede girar, salir de la función
            Vector3? turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {
                return;
            }
            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up)
                * movementDirection;
            Turn(context.ReadValue<float>(), turnPosition.Value);
            StartCoroutine(TurnCooldown()); // Iniciar cooldown después de girar
            
        }

        private void Turn(float turnValue, Vector3 turnPosition)
        {
            Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
            controller.enabled = false;
            transform.position = tempPlayerPosition;
            controller.enabled = true;

            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            // Restablecer la dirección de movimiento a cero después de un giro
            movementDirection = Vector3.zero;
        }

        private Vector3? CheckTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;

                if ((type == TileType.LEFT && turnValue == -1) ||
                    (type == TileType.RIGHT && turnValue == 1) ||
                    (type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }
            return null;
        }

        private void PlayerJump(InputAction.CallbackContext context)
        {
            if (controller.isGrounded)
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * currentGravity * -3f);
                controller.Move(playerVelocity * Time.deltaTime);
            }
        }

        private void PlayerSlide(InputAction.CallbackContext context)
        {
            if (!isSliding && controller.isGrounded)
            {
                StartCoroutine(Slide());
            }
        }

        private IEnumerator Slide()
        {
            isSliding = true;

            Vector3 originalControllerCenter = controller.center;
            Vector3 newControllerCenter = originalControllerCenter;
            controller.height /= 2;
            newControllerCenter.y -= controller.height / 2;
            controller.center = newControllerCenter;

            yield return new WaitForSeconds(1.3f);

            controller.height *= 2;
            controller.center = originalControllerCenter;
            isSliding = false;
        }

        private void StartMoving(InputAction.CallbackContext context)
        {
            float input = context.ReadValue<float>();
            movementDirection = input > 0 ? transform.right : -transform.right;
        }

        private void StopMoving(InputAction.CallbackContext context)
        {
            // Detener el movimiento
            movementDirection = Vector3.zero;
        }

        private void Update()
        {
            if (!PlayerManager.isGameStarted)
            {
                return;
            }
        
            if (Input.GetKeyUp(KeyCode.G))
            {
                godMode = !godMode;
            }
        
            if (Input.GetKeyUp(KeyCode.P))
            {
                PlayerManager.paused = !PlayerManager.paused;
            }
        
            if (playerSpeed < maximumPlayerSpeed)
            {
                playerSpeed += playerSpeedIncreaseRate * Time.deltaTime;
            }

        
            // Aplicar el movimiento horizontal
            controller.Move(movementDirection * horizontalSpeed * Time.deltaTime);
        
            // Aplicar el movimiento hacia adelante
            controller.Move(transform.forward * playerSpeed * Time.deltaTime);

            
        
            if (controller.isGrounded && playerVelocity.y < 0)
            {
                playerVelocity.y = 0;
            }
        
            playerVelocity.y += currentGravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        
            DetectAndJumpOverObstacles();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.transform.CompareTag("Obstacle") || hit.transform.CompareTag("ObstacleSlide"))
            {
                if (!godMode) {
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
                        if (!godMode) {
                            PlayerManager.gameOver = true;
                            FindObjectOfType<AudioManager>().Play("GameOver");
                            FindObjectOfType<AudioManager>().Stop("MainTheme");
                        }
                    }
                    StartCoroutine(CollisionCooldown());
                }
            }



                if(godMode) {
                    originalDirection = movementDirection; // Store the original direction

                    if (other.CompareTag("HoleLeft"))
                    {
                        if (controller.isGrounded)
                    {
                        PlayerJump(new InputAction.CallbackContext()); // Llamar al método PlayerJump
                        return; // Salir de la función para evitar comprobar otras alturas
                    }
                    }   
                    if (other.CompareTag("HoleRight"))
                    {
                        if (controller.isGrounded)
                    {
                        PlayerJump(new InputAction.CallbackContext()); // Llamar al método PlayerJump
                        return; // Salir de la función para evitar comprobar otras alturas
                    }
                    }
                }
            }

            private IEnumerator RestoreOriginalDirection()
            {
                // Wait a bit before restoring the original direction
                yield return new WaitForSeconds(0.2f);
                movementDirection = originalDirection;
            }


        private void DetectAndJumpOverObstacles()
        {
            if (!godMode) return; // Si el modo dios está desactivado, salir de la función

            // Definir el punto de inicio del Raycast en la posición de la hitbox del jugador
            Vector3 rayStart = controller.bounds.center;

            // Raycast principal
            RaycastHit hit;
            Debug.DrawRay(rayStart, transform.forward * obstacleDetectionDistance, Color.red);

            if (Physics.Raycast(rayStart, transform.forward, out hit, obstacleDetectionDistance))
            {
                if (hit.transform.CompareTag("Obstacle"))
                {
                    if (controller.isGrounded)
                    {
                        PlayerJump(new InputAction.CallbackContext()); // Llamar al método PlayerJump
                        return; // Salir de la función para evitar comprobar otras alturas
                    }
                }
            }

            rayStart = controller.bounds.center + Vector3.up * raycastHeightOffset; // Ajustar la altura usando el parámetro
            Debug.DrawRay(rayStart, transform.forward * obstacleDetectionDistance, Color.blue);

            if (Physics.Raycast(rayStart, transform.forward, out hit, obstacleDetectionDistance))
            {
                if (hit.transform.CompareTag("ObstacleSlide"))
                {
                    if (controller.isGrounded)
                    {
                        PlayerSlide(new InputAction.CallbackContext());
                        return; // Salir de la función para evitar comprobar otras alturas
                    }
                }
                OnTriggerEnter(hit.collider);
            }

            // Detectar y realizar giro automático
            if (canTurn)
            {
                Collider[] turnColliders = Physics.OverlapSphere(transform.position, .1f, turnLayer);
                foreach (Collider turnCollider in turnColliders)
                {
                    Tile tile = turnCollider.transform.parent.GetComponent<Tile>();
                    TileType type = tile.type;

                    if (type == TileType.LEFT)
                    {
                        Turn(-1, tile.pivot.position);
                        StartCoroutine(TurnCooldown());
                    }
                    else if (type == TileType.RIGHT)
                    {
                        Turn(1, tile.pivot.position);
                        StartCoroutine(TurnCooldown());
                    }
                }
            }
        }

        private IEnumerator TurnCooldown()
        {
            canTurn = false;
            yield return new WaitForSeconds(turnCooldown);
            canTurn = true;
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
}

