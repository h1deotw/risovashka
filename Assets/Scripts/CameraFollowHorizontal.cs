using UnityEngine;

public class CameraFollowHorizontal : MonoBehaviour
{
    [Header("Цель слежения")]
    public Transform player;

    [Header("Размер мёртвой зоны (по горизонтали)")]
    public float deadZoneWidth = 3f;   // Ширина зоны, где камера не двигается

    [Header("Плавность следования (при выходе за зону)")]
    public float followSmoothTime = 0.2f; // время сглаживания (меньше = быстрее, но может быть резко)

    [Header("Центрирование при остановке")]
    public bool enableCentering = true;
    public float centeringDelay = 0.5f;   // задержка перед центрированием
    public float centeringSmoothTime = 0.3f; // плавность центрирования

    private Rigidbody2D playerRb;
    private float timeSinceLastMove;
    private float currentVelocityX;        // для SmoothDamp
    private float targetCameraX;           // целевая позиция камеры по X
    private bool isCentering = false;

    void Start()
    {
        if (player != null)
        {
            playerRb = player.GetComponent<Rigidbody2D>();
            // Изначально камера смотрит на игрока по горизонтали
            Vector3 pos = transform.position;
            pos.x = player.position.x;
            transform.position = pos;
            targetCameraX = pos.x;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Проверяем, движется ли игрок горизонтально
        bool isMoving = (playerRb != null && Mathf.Abs(playerRb.linearVelocity.x) > 0.1f);

        if (isMoving)
        {
            timeSinceLastMove = 0f;
            isCentering = false;
        }
        else
        {
            timeSinceLastMove += Time.deltaTime;
        }

        // Получаем текущую позицию камеры по X
        float currentCameraX = transform.position.x;
        float playerX = player.position.x;

        // Режим центрирования (только когда игрок стоит и прошла задержка)
        if (enableCentering && !isMoving && timeSinceLastMove >= centeringDelay && !isCentering)
        {
            isCentering = true;
        }

        if (isCentering)
        {
            // Плавно двигаем камеру к позиции игрока по горизонтали
            targetCameraX = playerX;
            float newX = Mathf.SmoothDamp(currentCameraX, targetCameraX, ref currentVelocityX, centeringSmoothTime);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            // Если почти пришли — прекращаем центрирование
            if (Mathf.Abs(newX - playerX) < 0.02f)
            {
                isCentering = false;
                targetCameraX = playerX;
                transform.position = new Vector3(playerX, transform.position.y, transform.position.z);
            }
        }
        else
        {
            // Обычный режим с мёртвой зоной
            float leftBound = currentCameraX - deadZoneWidth * 0.5f;
            float rightBound = currentCameraX + deadZoneWidth * 0.5f;

            // Если игрок вышел за пределы мёртвой зоны
            if (playerX > rightBound)
            {
                targetCameraX = playerX - deadZoneWidth * 0.5f;
                float newX = Mathf.SmoothDamp(currentCameraX, targetCameraX, ref currentVelocityX, followSmoothTime);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
            else if (playerX < leftBound)
            {
                targetCameraX = playerX + deadZoneWidth * 0.5f;
                float newX = Mathf.SmoothDamp(currentCameraX, targetCameraX, ref currentVelocityX, followSmoothTime);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
        }
    }

    // Визуализация мёртвой зоны (зелёный прямоугольник)
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireCube(transform.position, new Vector3(deadZoneWidth, 0, 0));
    }
}