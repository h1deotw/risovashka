using UnityEngine;
using TMPro;
using System.Collections;

public class RopeActivator : MonoBehaviour
{
    [Header("Настройки подсказки [E]")]
    public Vector3 смещениеПодсказки = new Vector3(0, 1.5f, 0);
    public float скоростьПоявления = 0.5f;

    [Header("Анимация игрока")]
    public string анимацияИгрока = "Climb";

    [Header("Карабканье")]
    public ClimbRope скриптКарабканья;

    [Header("Задержки")]
    public float задержкаПередКарабканьем = 0.1f;

    [Header("=== ПЕРЕХОД НА СЛЕДУЮЩИЙ УРОВЕНЬ ===")]
    public string имяСледующегоУровня = "Level_2";
    public float задержкаПередПереходом = 0.5f;

    private static TMP_Text общаяПодсказка;
    private static CanvasGroup общийCanvasGroup;
    private static Camera основнаяКамера;

    private bool игрокВЗоне = false;
    private bool использовано = false;
    private bool игрокНаВерху = false;
    private bool переходАктивирован = false;
    private PlayerController cachedPlayerController;
    private GameObject текущийИгрок;

    void Start()
    {
        if (основнаяКамера == null) основнаяКамера = Camera.main;

        if (общаяПодсказка == null)
        {
            GameObject текстОбъект = GameObject.Find("E_Text");
            if (текстОбъект != null)
            {
                общаяПодсказка = текстОбъект.GetComponent<TMP_Text>();
                общийCanvasGroup = текстОбъект.GetComponent<CanvasGroup>();
                if (общийCanvasGroup == null)
                    общийCanvasGroup = текстОбъект.AddComponent<CanvasGroup>();
                общийCanvasGroup.alpha = 0f;
                общаяПодсказка.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (использовано) return;
        
        игрокВЗоне = true;
        текущийИгрок = other.gameObject;
        cachedPlayerController = текущийИгрок.GetComponent<PlayerController>();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        игрокВЗоне = false;
        текущийИгрок = null;
    }

    void Update()
    {
        if (общаяПодсказка == null || общийCanvasGroup == null) return;

        // Показываем подсказку [E] для старта карабканья
        bool нужно_показывать = игрокВЗоне && !использовано;

        // ИЛИ показываем подсказку [E] для перехода, если игрок на верху
        if (игрокНаВерху && !переходАктивирован && общаяПодсказка != null)
        {
            нужно_показывать = true;
        }

        float целеваяАльфа = нужно_показывать ? 1f : 0f;
        общийCanvasGroup.alpha = Mathf.Lerp(общийCanvasGroup.alpha, целеваяАльфа, Time.deltaTime * скоростьПоявления);

        if (общийCanvasGroup.alpha > 0.01f)
        {
            Vector3 мирПоз = transform.position + смещениеПодсказки;
            Vector3 экранПоз = основнаяКамера.WorldToScreenPoint(мирПоз);
            общаяПодсказка.transform.position = экранПоз;
        }

        // ===== ОБРАБОТКА НАЖАТИЯ E =====
        if (нужно_показывать && Input.GetKeyDown(KeyCode.E) && !использовано)
        {
            // Если игрок на верху — переходим на следующий уровень
            if (игрокНаВерху && !переходАктивирован)
            {
                ПерейтиНаСледующийУровень();
                return;
            }
            
            // Иначе начинаем карабканье
            StartCoroutine(АктивироватьВеревку());
        }
    }

    IEnumerator АктивироватьВеревку()
    {
        использовано = true;
        игрокВЗоне = false;

        if (общийCanvasGroup != null)
            общийCanvasGroup.alpha = 0f;

        if (cachedPlayerController != null)
            cachedPlayerController.BlockMovement();

        // Телепорт + анимация одновременно
        if (скриптКарабканья != null && скриптКарабканья.точкаСтарт != null)
        {
            текущийИгрок.transform.position = скриптКарабканья.точкаСтарт.position;
            Debug.Log("[RopeActivator] Игрок телепортирован на старт!");
        }

        if (!string.IsNullOrEmpty(анимацияИгрока) && текущийИгрок != null)
        {
            Animator anim = текущийИгрок.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play(анимацияИгрока);
                Debug.Log($"[RopeActivator] Запущена анимация: {анимацияИгрока}");
            }
        }

        yield return new WaitForSeconds(задержкаПередКарабканьем);

        if (скриптКарабканья != null)
        {
            Debug.Log("[RopeActivator] Запускаю карабканье!");
            скриптКарабканья.НачатьКарабканье();
        }
        else
        {
            if (cachedPlayerController != null)
                cachedPlayerController.UnblockMovement();
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;
    }

    // ===== МЕТОД ДЛЯ ВЫЗОВА ИЗ ClimbRope КОГДА ИГРОК ДОСТИГ ВЕРХА =====
    public void ИгрокДостигВерха()
    {
        игрокНаВерху = true;
        Debug.Log("[RopeActivator] Игрок на верху! Нажмите E для перехода.");
        
        // Показываем подсказку [E]
        if (общийCanvasGroup != null)
        {
            общийCanvasGroup.alpha = 1f;
        }
    }

    // ===== ПЕРЕХОД НА СЛЕДУЮЩИЙ УРОВЕНЬ =====
    void ПерейтиНаСледующийУровень()
    {
        if (переходАктивирован) return;
        переходАктивирован = true;

        Debug.Log($"[RopeActivator] Загрузка уровня: {имяСледующегоУровня}");
        
        // Скрываем подсказку
        if (общийCanvasGroup != null)
            общийCanvasGroup.alpha = 0f;

        Invoke(nameof(ЗагрузитьУровень), задержкаПередПереходом);
    }

    void ЗагрузитьУровень()
    {
        LevelManager lm = FindFirstObjectByType<LevelManager>();
        if (lm != null)
        {
            lm.LoadScene(имяСледующегоУровня);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(имяСледующегоУровня);
        }
    }
}