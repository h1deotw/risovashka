using UnityEngine;

public class ObjectAnimator : MonoBehaviour
{
    [Header("Аниматор для игрока после превращения")]
    public RuntimeAnimatorController animatorController;

    private GameObject player;
    private Animator playerAnimator;
    private bool isPlayer = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player не найден! У игрока должен быть тег 'Player'.");
            return;
        }
        
        playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator == null)
        {
            Debug.LogError("У игрока нет компонента Animator!");
        }
    }

    void Update()
    {
        if (player == null || playerAnimator == null) return;
        
        // Если аниматор уже заменён — передаём параметры движения
        if (playerAnimator.runtimeAnimatorController == animatorController)
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float lastMoveX = moveX != 0 ? moveX : playerAnimator.GetFloat("LastMoveX");
            
            playerAnimator.SetFloat("MoveX", moveX);
            playerAnimator.SetFloat("LastMoveX", lastMoveX);
        }
    }

    // Этот метод вызывает SimplePrompt при превращении
    public void ApplyToPlayer()
    {
        if (playerAnimator != null && animatorController != null)
        {
            playerAnimator.runtimeAnimatorController = animatorController;
            Debug.Log($"Аниматор игрока заменён на {animatorController.name}");
        }
        else
        {
            Debug.LogWarning("Animator Controller не назначен!");
        }
    }
}