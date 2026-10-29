using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Движение")]
    [SerializeField] private float moveSpeed = 5f;
    
    [Header("Состояние")]
    public bool isTransformed = false;
    
    private Rigidbody2D rb;
    private Animator anim;
    private bool canMove = true;
    private Vector2 moveInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        
        if (rb == null)
            Debug.LogError("[PlayerController] Rigidbody2D не найден!");
    }
    
    void Update()
    {
        // Если движение заблокировано - не двигаемся
        if (!canMove) 
        {
            // Останавливаем анимацию движения
            if (anim != null)
            {
                anim.SetFloat("MoveX", 0);
                anim.SetFloat("MoveY", 0);
                anim.SetBool("IsMoving", false);
            }
            return;
        }
        
        // Получаем ввод с клавиатуры
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        
        // Двигаем игрока
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        
        // Анимация
        if (anim != null)
        {
            anim.SetFloat("MoveX", moveInput.x);
            anim.SetFloat("MoveY", 0);
            anim.SetBool("IsMoving", Mathf.Abs(moveInput.x) > 0.1f);
        }
    }
    
    // Блокировка движения игрока
    public void BlockMovement()
    {
        canMove = false;
        
        // Останавливаем физическое движение
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        Debug.Log("[PlayerController] Движение ЗАБЛОКИРОВАНО");
    }
    
    // Разблокировка движения игрока
    public void UnblockMovement()
    {
        canMove = true;
        Debug.Log("[PlayerController] Движение РАЗБЛОКИРОВАНО");
    }
    
    // Проверка - может ли игрок двигаться
    public bool CanMove()
    {
        return canMove;
    }
    
    // Сброс превращения
    public void ResetTransformation()
    {
        isTransformed = false;
        Debug.Log("[PlayerController] Трансформация сброшена");
    }
}