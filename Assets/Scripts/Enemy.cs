using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TempleRun.Player;

public class Enemy : MonoBehaviour
{
    public Transform player;
    public Animator animator;
    private float baseSpeed = 5;
    private float currentSpeed;
    private float followDistance = 4; // La distancia que el enemigo mantendrá detrás del jugador

    private Quaternion rotationOffset = Quaternion.Euler(0, 180, 180); // Ajusta estos valores según sea necesario

    // Start is called before the first frame update
    void Start()
    {
        // Asegúrate de que el enemigo comience con la misma rotación que el jugador ajustada por la rotación offset
        transform.rotation = player.rotation * rotationOffset;
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetBool("isGameStarted", true);

        if (PC2.hurt) {
            // Ajusta la velocidad del enemigo en función de la velocidad del jugador
            currentSpeed = baseSpeed + PlayerMove.forwardSpeed;

            // Ajusta la posición de seguimiento para que esté detrás del jugador
            Vector3 followPosition = player.position - player.forward * followDistance;
            followPosition.y = transform.position.y; // Mantén la posición y del enemigo constante
            transform.position = Vector3.Lerp(transform.position, followPosition, currentSpeed * Time.deltaTime);

            // Actualiza la rotación del enemigo para que coincida con la del jugador ajustada por la rotación offset
            transform.rotation = Quaternion.Lerp(transform.rotation, player.rotation * rotationOffset, currentSpeed * Time.deltaTime);
        }
    }
}
