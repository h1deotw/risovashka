using UnityEngine;
using TMPro;
using System.Collections;

public class InteractableActivator : MonoBehaviour
{
    [Header("Целевые объекты (будут АКТИВИРОВАНЫ при нажатии E)")]
    public GameObject[] объектыДляАктивации;

    [Header("Объекты, у которых нужно включить SpriteRenderer (а не сам объект)")]
    public SpriteRenderer[] спрайтРендерыДляВключения;

    [Header("Объекты, которые будут ДЕАКТИВИРОВАНЫ (исчезнут) при нажатии E")]
    public GameObject[] объектыДляДеактивации;

    [Header("SpriteRenderer, которые будут ВЫКЛЮЧЕНЫ при нажатии E")]
    public SpriteRenderer[] спрайтРендерыДляВыключения;

    [Header("Настройки подсказки [E]")]
    public Vector3 смещениеПодсказки = new Vector3(0, 1.5f, 0);
    public float скоростьПоявления = 5f;

    [Header("=== УСЛОВИЯ ПОКАЗА ПОДСКАЗКИ ===")]
    public bool требоватьПревращение = true;

    [Header("=== КОНКРЕТНЫЙ ОБЪЕКТ ДЛЯ ТРАНСФОРМАЦИИ ===")]
    [Tooltip("Игрок должен быть превращён именно в этот объект, чтобы нажать E. Оставь пустым - любой объект трансформации подойдёт.")]
    public GameObject требуемыйОбъектТрансформации;

    [Header("=== ВОЗВРАТ В ИГРОКА ПОСЛЕ АКТИВАЦИИ ===")]
    public bool вернутьИгрокаВИсходное = true;

    [Header("=== АНИМАЦИЯ ПРИ АКТИВАЦИИ ===")]
    [Tooltip("Анимация на ИГРОКЕ (например: Hammer_Attack). Оставь пустым, если не нужна")]
    public string анимацияИгрока = "";

    [Tooltip("Анимация на ЭТОМ ОБЪЕКТЕ (например: Open). Оставь пустым, если не нужна")]
    public string анимацияОбъекта = "";

    [Tooltip("Запасная длительность, если анимацию не удалось определить")]
    public float запаснаяДлительность = 0.5f;

    [Header("=== КАРАБКАНЬЕ ПО ВЕРЕВКЕ ===")]
    [Tooltip("Скрипт карабканья (ClimbRope), который будет запущен после анимации")]
    public ClimbRope скриптКарабканья;

    private Sprite игрокИсходныйСпрайт;
    private Color игрокИсходныйЦвет;
    private Vector2 игрокИсходныйРазмерКоллайдера;
    private RuntimeAnimatorController игрокИсходныйАниматор;

    private static TMP_Text общаяПодсказка;
    private static CanvasGroup общийCanvasGroup;
    private static Camera основнаяКамера;

    private bool игрокВЗоне = false;
    private bool использовано = false;
    private Collider2D триггерКоллайдер;
    private bool игрокПревратился = false;
    
    private PlayerController cachedPlayerController;
    private GameObject текущийИгрок;

    void Start()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"[InteractableActivator] Объект {gameObject.name} НЕАКТИВЕН! Включаю...");
            gameObject.SetActive(true);
        }

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

        СохранитьИсходноеСостояниеИгрока();

        foreach (GameObject obj in объектыДляАктивации)
            if (obj != null) obj.SetActive(false);

        foreach (SpriteRenderer sr in спрайтРендерыДляВключения)
            if (sr != null) sr.enabled = false;

        триггерКоллайдер = GetComponent<Collider2D>();
    }

    void СохранитьИсходноеСостояниеИгрока()
    {
        GameObject игрок = GameObject.FindGameObjectWithTag("Player");
        if (игрок == null) return;

        текущийИгрок = игрок;
        cachedPlayerController = игрок.GetComponent<PlayerController>();

        SpriteRenderer sr = игрок.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            игрокИсходныйСпрайт = sr.sprite;
            игрокИсходныйЦвет = sr.color;
        }

        BoxCollider2D col = игрок.GetComponent<BoxCollider2D>();
        if (col != null)
            игрокИсходныйРазмерКоллайдера = col.size;

        Animator anim = игрок.GetComponent<Animator>();
        if (anim != null)
            игрокИсходныйАниматор = anim.runtimeAnimatorController;
    }

    void ВернутьИгрокаВИсходноеСостояние()
    {
        GameObject игрок = GameObject.FindGameObjectWithTag("Player");
        if (игрок == null) return;

        SpriteRenderer sr = игрок.GetComponent<SpriteRenderer>();
        if (sr != null && игрокИсходныйСпрайт != null)
        {
            sr.sprite = игрокИсходныйСпрайт;
            sr.color = игрокИсходныйЦвет;
        }

        BoxCollider2D col = игрок.GetComponent<BoxCollider2D>();
        if (col != null)
            col.size = игрокИсходныйРазмерКоллайдера;

        Animator anim = игрок.GetComponent<Animator>();
        if (anim != null && игрокИсходныйАниматор != null)
            anim.runtimeAnimatorController = игрокИсходныйАниматор;

        PlayerController pc = игрок.GetComponent<PlayerController>();
        if (pc != null)
            pc.isTransformed = false;
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
        игрокПревратился = false;
        текущийИгрок = null;
    }

    void Update()
    {
        if (общаяПодсказка == null || общийCanvasGroup == null) return;

        bool нужно_показывать = false;
        
        if (игрокВЗоне && !использовано)
        {
            bool превратился = false;
            
            if (cachedPlayerController != null)
            {
                превратился = cachedPlayerController.isTransformed;
            }
            
            if (требоватьПревращение && требуемыйОбъектТрансформации != null)
            {
                SpriteRenderer sr = текущийИгрок?.GetComponent<SpriteRenderer>();
                SpriteRenderer требуемыйSr = требуемыйОбъектТрансформации?.GetComponent<SpriteRenderer>();
                
                bool совпадаетСпрайт = false;
                if (sr != null && требуемыйSr != null)
                {
                    совпадаетСпрайт = (sr.sprite == требуемыйSr.sprite);
                }
                
                нужно_показывать = превратился && совпадаетСпрайт;
            }
            else if (требоватьПревращение)
            {
                нужно_показывать = превратился;
            }
            else
            {
                нужно_показывать = true;
            }
        }

        float целеваяАльфа = нужно_показывать ? 1f : 0f;
        общийCanvasGroup.alpha = Mathf.Lerp(общийCanvasGroup.alpha, целеваяАльфа, Time.deltaTime * скоростьПоявления);

        if (общийCanvasGroup.alpha > 0.01f)
        {
            Vector3 мирПоз = transform.position + смещениеПодсказки;
            Vector3 экранПоз = основнаяКамера.WorldToScreenPoint(мирПоз);
            общаяПодсказка.transform.position = экранПоз;
        }

        if (нужно_показывать && Input.GetKeyDown(KeyCode.E) && !использовано)
        {
            StartCoroutine(ПроигратьАнимациюИАктивировать());
        }
    }

    IEnumerator ПроигратьАнимациюИАктивировать()
    {
        float максимальнаяДлительность = 0f;
        
        PlayerController pc = null;
        if (cachedPlayerController != null)
        {
            pc = cachedPlayerController;
            pc.BlockMovement();
        }
        else
        {
            GameObject игрок = GameObject.FindGameObjectWithTag("Player");
            if (игрок != null)
            {
                pc = игрок.GetComponent<PlayerController>();
                if (pc != null) pc.BlockMovement();
            }
        }

        if (!string.IsNullOrEmpty(анимацияИгрока))
        {
            GameObject игрок = GameObject.FindGameObjectWithTag("Player");
            if (игрок != null)
            {
                Animator anim = игрок.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.Play(анимацияИгрока);
                    float длина = НайтиДлинуАнимации(anim, анимацияИгрока);
                    if (длина > максимальнаяДлительность)
                        максимальнаяДлительность = длина;
                }
            }
        }

        if (!string.IsNullOrEmpty(анимацияОбъекта))
        {
            Animator objAnim = GetComponent<Animator>();
            if (objAnim != null)
            {
                objAnim.Play(анимацияОбъекта);
                float длина = НайтиДлинуАнимации(objAnim, анимацияОбъекта);
                if (длина > максимальнаяДлительность)
                    максимальнаяДлительность = длина;
            }
        }

        if (максимальнаяДлительность > 0f)
            yield return new WaitForSeconds(максимальнаяДлительность);
        else if (запаснаяДлительность > 0f)
            yield return new WaitForSeconds(запаснаяДлительность);

        foreach (GameObject obj in объектыДляАктивации)
            if (obj != null) obj.SetActive(true);

        foreach (SpriteRenderer sr in спрайтРендерыДляВключения)
            if (sr != null) sr.enabled = true;

        foreach (GameObject obj in объектыДляДеактивации)
            if (obj != null) obj.SetActive(false);

        foreach (SpriteRenderer sr in спрайтРендерыДляВыключения)
            if (sr != null) sr.enabled = false;

        if (вернутьИгрокаВИсходное)
            ВернутьИгрокаВИсходноеСостояние();

        if (скриптКарабканья != null)
        {
            Debug.Log("[InteractableActivator] Запускаю карабканье!");
            скриптКарабканья.НачатьКарабканье();
        }

        // ===== ВЫЗЫВАЕМ ПОДСКАЗКУ =====
        TaksaManager taksa = FindFirstObjectByType<TaksaManager>();
        if (taksa != null)
        {
            taksa.НаНажатиеE(gameObject);
            
            // ===== ЕСЛИ ЭТО InteractableObject (2) — ПРИНУДИТЕЛЬНО УБИРАЕМ =====
            if (gameObject.name == "InteractableObject (2)")
            {
                taksa.УбратьМгновенно();
                Debug.Log("[InteractableActivator] Принудительно убрал таксу!");
            }
            // ================================================================
        }
        // ===============================

        if (скриптКарабканья == null && pc != null)
            pc.UnblockMovement();

        использовано = true;
        игрокВЗоне = false;
        игрокПревратился = false;

        if (общийCanvasGroup != null)
            общийCanvasGroup.alpha = 0f;

        if (триггерКоллайдер != null)
            триггерКоллайдер.enabled = false;

        this.enabled = false;
    }

    float НайтиДлинуАнимации(Animator anim, string имяАнимации)
    {
        if (anim == null || string.IsNullOrEmpty(имяАнимации)) return запаснаяДлительность;
        
        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        foreach (var clip in clips)
        {
            if (clip.name == имяАнимации)
                return clip.length;
        }
        return запаснаяДлительность;
    }
}