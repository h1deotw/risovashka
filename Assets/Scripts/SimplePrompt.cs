using UnityEngine;
using TMPro;
using System.Collections;

public class SimplePrompt : MonoBehaviour
{
    public enum InteractionType { Transform, Minigame, Change }

    [Header("Тип взаимодействия")]
    public InteractionType interactionType = InteractionType.Transform;

    [Header("Настройки подсказки (только для Minigame/Change)")]
    public Vector3 manualOffset = Vector3.zero;
    public float fadeSpeed = 5f;

    [Header("Настройки превращения (Transform)")]
    public float holdDuration = 1f;
    public bool showProgressUI = true;

    private static TMP_Text sharedPrompt;
    private static CanvasGroup sharedCanvasGroup;
    private static Camera mainCamera;

    private Vector3 calculatedOffset;
    private Coroutine currentFade;
    private bool playerInRange = false;

    private bool isMouseOver = false;
    private bool isTransforming = false;
    private float currentHoldTime = 0f;
    private bool isHolding = false;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;

        if (sharedPrompt == null)
        {
            GameObject textObj = GameObject.Find("E_Text");
            if (textObj != null)
            {
                sharedPrompt = textObj.GetComponent<TMP_Text>();
                sharedCanvasGroup = textObj.GetComponent<CanvasGroup>();
                if (sharedCanvasGroup == null)
                    sharedCanvasGroup = textObj.AddComponent<CanvasGroup>();
                sharedCanvasGroup.alpha = 0f;
                sharedPrompt.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("[SimplePrompt] E_Text не найден в сцене!");
            }
        }

        float height = 0f;
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) height = col.bounds.size.y;
        else
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null) height = sr.bounds.size.y;
        }
        calculatedOffset = new Vector3(0, height / 2f + 0.5f, 0);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        if (interactionType != InteractionType.Transform)
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(Fade(0f, 1f));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        if (interactionType != InteractionType.Transform)
        {
            if (currentFade != null) StopCoroutine(currentFade);
            currentFade = StartCoroutine(Fade(1f, 0f));
        }
    }

    void OnMouseEnter()
    {
        if (interactionType == InteractionType.Transform)
            isMouseOver = true;
    }

    void OnMouseExit()
    {
        if (interactionType == InteractionType.Transform)
        {
            isMouseOver = false;
            ResetHold();
        }
    }

    void Update()
    {
        if (interactionType != InteractionType.Transform &&
            sharedCanvasGroup != null && sharedCanvasGroup.alpha > 0.01f)
        {
            UpdatePosition();
        }

        if (playerInRange && Input.GetKeyDown(KeyCode.E) && interactionType != InteractionType.Transform)
        {
            Interact();
        }

        if (interactionType == InteractionType.Transform && isMouseOver && !isTransforming)
        {
            bool isLeftMousePressed = Input.GetMouseButton(0);
            PlayerController pc = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
            
            if (isLeftMousePressed)
            {
                if (pc != null)
                {
                    if (!isHolding)
                        pc.BlockMovement();
                }
                
                if (!isHolding)
                {
                    isHolding = true;
                    currentHoldTime = 0f;
                    ЗапуститьАнимациюТрансформацииНаИгроке(true);
                }
                
                currentHoldTime += Time.deltaTime;
                
                if (showProgressUI)
                {
                    Debug.Log($"Удержание: {currentHoldTime / holdDuration * 100:F0}%");
                }
                
                if (currentHoldTime >= holdDuration)
                {
                    TransformInto();
                    ResetHold();
                }
            }
            else if (isHolding && !isLeftMousePressed)
            {
                if (pc != null)
                    pc.UnblockMovement();
                
                ЗапуститьАнимациюТрансформацииНаИгроке(false);
                ResetHold();
                if (showProgressUI) Debug.Log("Удержание прервано! Движение разблокировано.");
            }
        }
        else if (isHolding)
        {
            PlayerController pc = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
            if (pc != null)
                pc.UnblockMovement();
            
            ЗапуститьАнимациюТрансформацииНаИгроке(false);
            ResetHold();
        }
    }

    public bool IsPlayerInRange()
    {
        return playerInRange;
    }

    void ЗапуститьАнимациюТрансформацииНаИгроке(bool запустить)
    {
        GameObject игрок = GameObject.FindGameObjectWithTag("Player");
        if (игрок == null) return;
        
        Animator anim = игрок.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("IsTransforming", запустить);
            
            if (запустить)
                anim.Play("Transform");
            else
                anim.Play("Idle");
        }
    }

    void ResetHold()
    {
        isHolding = false;
        currentHoldTime = 0f;
        
        PlayerController pc = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
        if (pc != null)
            pc.UnblockMovement();
    }

    void UpdatePosition()
    {
        if (sharedPrompt == null) return;
        Vector3 worldPos = transform.position + calculatedOffset + manualOffset;
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);
        sharedPrompt.transform.position = screenPos;
    }

    void Interact()
    {
        switch (interactionType)
        {
            case InteractionType.Minigame:
                Debug.Log($"[SimplePrompt] Нажатие E на {gameObject.name}");
                
                MinigameCycle cycle = GetComponent<MinigameCycle>();
                if (cycle == null)
                    cycle = GetComponentInChildren<MinigameCycle>();
                
                if (cycle != null)
                {
                    Debug.Log("[SimplePrompt] MinigameCycle найден, передаю нажатие");
                    
                    if (!cycle.isGameActive())
                    {
                        Debug.Log("[SimplePrompt] Запускаем мини-игру");
                        cycle.StartMinigame();
                    }
                    else
                    {
                        Debug.Log("[SimplePrompt] Мини-игра активна, пробуем поймать объект");
                        cycle.CatchCurrentObject();
                    }
                }
                else
                {
                    Debug.LogError($"[SimplePrompt] MinigameCycle НЕ НАЙДЕН на объекте {gameObject.name}!");
                }
                break;
                
            case InteractionType.Change:
                Debug.Log($"[SimplePrompt] Изменение объекта {gameObject.name}");
                break;
        }
    }

    void TransformInto()
    {
        if (isTransforming) return;
        isTransforming = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Игрок не найден! У объекта Player должен быть тег 'Player'.");
            isTransforming = false;
            return;
        }
        
        ЗапуститьАнимациюТрансформацииНаИгроке(false);
        
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.isTransformed = true;
        }
        
        SpriteRenderer objectSprite = GetComponent<SpriteRenderer>();
        if (objectSprite != null)
        {
            SpriteRenderer playerSprite = player.GetComponent<SpriteRenderer>();
            if (playerSprite != null)
            {
                playerSprite.sprite = objectSprite.sprite;
                playerSprite.color = objectSprite.color;
            }
        }
        else
        {
            Debug.LogWarning("У объекта нет SpriteRenderer, превращение невозможно.");
            isTransforming = false;
            return;
        }

        ObjectAnimator objAnim = GetComponent<ObjectAnimator>();
        if (objAnim != null)
        {
            objAnim.ApplyToPlayer();
        }
        
        if (playerController != null)
            playerController.UnblockMovement();

        // ===== ВЫЗЫВАЕМ ПОДСКАЗКУ =====
        TaksaManager taksa = FindFirstObjectByType<TaksaManager>();
        if (taksa != null)
        {
            if (gameObject.name.ToLower().Contains("feather") || 
                gameObject.name.ToLower().Contains("перо"))
            {
                taksa.НаПревращениеВПеро();
            }
            else
            {
                taksa.НаТрансформацию();
            }
        }
        // ===============================

        Destroy(gameObject);
    }

    IEnumerator Fade(float from, float to)
    {
        float time = 0f;
        float duration = 1f / fadeSpeed;
        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            if (sharedCanvasGroup != null)
                sharedCanvasGroup.alpha = Mathf.Lerp(from, to, t);
            yield return null;
        }
        if (sharedCanvasGroup != null) sharedCanvasGroup.alpha = to;
        currentFade = null;
    }
}