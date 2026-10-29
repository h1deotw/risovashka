using UnityEngine;
using System.Collections;

public class ClimbRope : MonoBehaviour
{
    [Header("Точки движения")]
    public Transform точкаСтарт;
    public Transform точкаФиниш;

    [Header("Игрок")]
    public GameObject игрок;

    [Header("Настройки карабканья")]
    public float скоростьКарабканья = 2f;
    public string анимацияКарабканья = "Climb";

    private bool карабканьеАктивно = false;
    private bool игрокНаВерху = false;
    private PlayerController playerController;
    private Animator playerAnimator;
    private RopeActivator ropeActivator;  // ← НОВОЕ

    void Start()
    {
        if (игрок == null)
            игрок = GameObject.FindGameObjectWithTag("Player");
        
        if (игрок != null)
        {
            playerController = игрок.GetComponent<PlayerController>();
            playerAnimator = игрок.GetComponent<Animator>();
        }

        // Находим RopeActivator на этом же объекте
        ropeActivator = GetComponent<RopeActivator>();
    }

    public void НачатьКарабканье()
    {
        if (карабканьеАктивно) return;
        if (игрок == null || точкаСтарт == null || точкаФиниш == null)
        {
            Debug.LogError("[ClimbRope] Не все настройки заполнены!");
            return;
        }

        карабканьеАктивно = true;
        StartCoroutine(Карабкаться());
    }

    IEnumerator Карабкаться()
    {
        if (playerController != null)
            playerController.BlockMovement();
        
        Debug.Log("[ClimbRope] Начинаю карабканье!");

        игрок.transform.position = точкаСтарт.position;
        
        if (playerAnimator != null && !string.IsNullOrEmpty(анимацияКарабканья))
        {
            playerAnimator.Play(анимацияКарабканья);
        }

        Vector3 старт = точкаСтарт.position;
        Vector3 финиш = точкаФиниш.position;
        
        float прогресс = 0f;
        float расстояние = Vector3.Distance(старт, финиш);
        float времяДвижения = расстояние / скоростьКарабканья;

        while (прогресс < 1f)
        {
            прогресс += Time.deltaTime / времяДвижения;
            прогресс = Mathf.Clamp01(прогресс);
            
            игрок.transform.position = Vector3.Lerp(старт, финиш, прогресс);
            
            yield return null;
        }

        игрок.transform.position = финиш;
        игрокНаВерху = true;
        
        if (playerAnimator != null)
        {
            playerAnimator.speed = 0f;
            Debug.Log("[ClimbRope] Анимация застыла на последнем кадре!");
        }

        Debug.Log("[ClimbRope] Игрок достиг верха!");

        // ===== ОПОВЕЩАЕМ RopeActivator ЧТО ИГРОК НА ВЕРХУ =====
        if (ropeActivator != null)
        {
            ropeActivator.ИгрокДостигВерха();
        }
        else
        {
            // Если RopeActivator нет — просто разблокируем игрока
            if (playerController != null)
                playerController.UnblockMovement();
        }

        карабканьеАктивно = false;
    }

    public bool IsPlayerAtTop()
    {
        return игрокНаВерху;
    }

    public bool IsClimbing()
    {
        return карабканьеАктивно;
    }
}