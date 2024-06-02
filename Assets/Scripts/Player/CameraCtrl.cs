using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    void Start()
    {
        // Puedes inicializar el offset en el editor de Unity o hacerlo aquí
        if (offset == Vector3.zero)
        {
            offset = new Vector3(0, 5, -10);
        }
    }

    void LateUpdate()
    {
        FollowTarget();
    }

    private void FollowTarget()
    {
        // Calcular la posición deseada
        Vector3 desiredPosition = target.position + target.rotation * offset;
        
        // Suavizar la transición de la posición actual a la deseada
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        
        // Actualizar la posición de la cámara
        transform.position = smoothedPosition;
        
        // Rotación adicional de 30 grados en el eje X
        Quaternion rotation = Quaternion.Euler(30, target.eulerAngles.y, 0);
        transform.rotation = rotation;

        // Asegura que la cámara siempre mire al personaje
        transform.LookAt(target);
    }
}
