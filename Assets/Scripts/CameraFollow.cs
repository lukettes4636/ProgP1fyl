using UnityEngine;
// Comentario: Sigue al jugador suavemente para centrar la cámara
// Comentario: Mantiene el desplazamiento (offset) configurado en el inspector

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    // Comentario: Actualiza en LateUpdate para evitar jitter con movimiento
    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
    }
}
