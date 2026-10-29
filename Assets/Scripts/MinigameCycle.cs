using UnityEngine;
using System.Collections;

public class MinigameCycle : MonoBehaviour
{
    [Header("Объекты для цикличного появления (порядок важен)")]
    public GameObject[] объектыДляЦикла;

    [Header("Настройки мигания объекта")]
    public float интервалМигания = 1f;
    public float скоростьАнимации = 0.2f;
    public bool показыватьСообщения = true;

    [Header("=== ТЕСТОВЫЙ РЕЖИМ ===")]
    public bool включитьТестовыйРежим = true;
    public KeyCode клавишаМгновеннойПобеды = KeyCode.Q;

    private int текущийИндекс = 0;
    private bool объектСейчасВиден = false;
    private bool миниИграАктивна = false;
    private bool анимацияИдет = false;
    private bool ожидаемСледующийЦикл = false;
    
    private SpriteRenderer[][] всеРендеры;
    private Coroutine миганиеКорутина;
    private PlayerController playerController;

    void Start()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError($"[MinigameCycle] Объект {gameObject.name} НЕАКТИВЕН! Включи его в иерархии.");
            return;
        }

        GameObject игрок = GameObject.FindGameObjectWithTag("Player");
        if (игрок != null)
            playerController = игрок.GetComponent<PlayerController>();

        if (объектыДляЦикла == null || объектыДляЦикла.Length == 0)
        {
            Debug.LogWarning("Нет объектов для цикла");
            return;
        }

        всеРендеры = new SpriteRenderer[объектыДляЦикла.Length][];
        
        for (int i = 0; i < объектыДляЦикла.Length; i++)
        {
            if (объектыДляЦикла[i] != null)
            {
                всеРендеры[i] = объектыДляЦикла[i].GetComponentsInChildren<SpriteRenderer>();
                SetAlpha(всеРендеры[i], 0f);
                объектыДляЦикла[i].SetActive(true);
            }
        }
        
        Debug.Log($"[MinigameCycle] Объект {gameObject.name} активен, готов к работе");
        
        if (включитьТестовыйРежим)
            Debug.Log($"[MinigameCycle] 🧪 ТЕСТОВЫЙ РЕЖИМ ВКЛЮЧЁН! Нажми {клавишаМгновеннойПобеды} для мгновенной победы!");
    }

    void Update()
    {
        if (включитьТестовыйРежим && миниИграАктивна && Input.GetKeyDown(клавишаМгновеннойПобеды))
        {
            Debug.Log("[MiniGame] 🚀 ТЕСТОВЫЙ РЕЖИМ: Мгновенная победа по клавише Q!");
            МгновеннаяПобеда();
        }
    }

    public bool isGameActive()
    {
        return миниИграАктивна;
    }

    public void StartMinigame()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[MinigameCycle] Нельзя запустить мини-игру на неактивном объекте!");
            return;
        }
        
        if (миниИграАктивна) return;
        
        миниИграАктивна = true;
        текущийИндекс = 0;
        
        if (playerController != null)
            playerController.BlockMovement();
        
        if (показыватьСообщения)
            Debug.Log($"[MiniGame] ИГРА НАЧАЛАСЬ! Лови объект 1");
        
        ЗапуститьМигание();
    }

    public void CatchCurrentObject()
    {
        if (!миниИграАктивна) 
        {
            if (показыватьСообщения)
                Debug.Log("[MiniGame] Игра не активна");
            return;
        }
        
        if (анимацияИдет || ожидаемСледующийЦикл)
        {
            if (показыватьСообщения)
                Debug.Log("[MiniGame] Подожди...");
            return;
        }

        if (объектСейчасВиден)
        {
            if (показыватьСообщения)
                Debug.Log($"[MiniGame] ✅ ПОЙМАЛ объект {текущийИндекс + 1}! Он остаётся видимым.");
            
            if (миганиеКорутина != null)
                StopCoroutine(миганиеКорутина);
            
            StartCoroutine(ЗафиксироватьИПереключить());
        }
        else
        {
            if (показыватьСообщения)
                Debug.Log($"[MiniGame] ❌ ПРОМАХ! Объект {текущийИндекс + 1} был невидим");
        }
    }

    void МгновеннаяПобеда()
    {
        if (!миниИграАктивна) return;
        
        if (миганиеКорутина != null)
            StopCoroutine(миганиеКорутина);
        
        for (int i = 0; i < объектыДляЦикла.Length; i++)
        {
            if (всеРендеры[i] != null)
            {
                SetAlpha(всеРендеры[i], 1f);
            }
        }
        
        текущийИндекс = объектыДляЦикла.Length;
        миниИграАктивна = false;
        
        Debug.Log($"[MiniGame] 🎉 ПОБЕДА! Все {объектыДляЦикла.Length} объектов пойманы! 🎉");
        
        ЗапуститьFrogПослеПобеды();
    }

    void ЗапуститьМигание()
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[MinigameCycle] Объект неактивен, не могу запустить мигание!");
            return;
        }
        
        if (миганиеКорутина != null)
            StopCoroutine(миганиеКорутина);
        миганиеКорутина = StartCoroutine(Мигать());
    }

    IEnumerator ЗафиксироватьИПереключить()
    {
        анимацияИдет = true;
        
        SpriteRenderer[] рендерыТекущего = всеРендеры[текущийИндекс];
        if (рендерыТекущего != null && рендерыТекущего.Length > 0)
        {
            float текущаяАльфа = рендерыТекущего[0].color.a;
            yield return StartCoroutine(ПлавнаяАльфа(рендерыТекущего, текущаяАльфа, 1f, скоростьАнимации));
        }
        
        текущийИндекс++;
        
        if (текущийИндекс < объектыДляЦикла.Length)
        {
            if (показыватьСообщения)
                Debug.Log($"[MiniGame] Теперь лови объект {текущийИндекс + 1}");
            
            объектСейчасВиден = false;
            ожидаемСледующийЦикл = false;
            ЗапуститьМигание();
        }
        else
        {
            миниИграАктивна = false;
            
            Debug.Log($"[MiniGame] 🎉 ПОБЕДА! Все {объектыДляЦикла.Length} объектов пойманы! 🎉");
            
            ЗапуститьFrogПослеПобеды();
        }
        
        анимацияИдет = false;
    }

    void ЗапуститьFrogПослеПобеды()
    {
        FrogSequence frog = FindObjectOfType<FrogSequence>();
        
        if (frog != null)
        {
            Debug.Log("[MiniGame] Запускаю последовательность Frog!");
            frog.НачатьПоследовательность();
        }
        else
        {
            Debug.LogWarning("[MiniGame] FrogSequence не найден на сцене!");
        }

        // ===== ВЫЗЫВАЕМ ПОДСКАЗКУ =====
        TaksaManager taksa = FindFirstObjectByType<TaksaManager>();
        if (taksa != null)
        {
            taksa.НаМиниИгруПобеда();
        }
        // ===============================
    }

    IEnumerator Мигать()
    {
        ожидаемСледующийЦикл = false;
        
        while (миниИграАктивна && текущийИндекс < объектыДляЦикла.Length)
        {
            SpriteRenderer[] рендеры = всеРендеры[текущийИндекс];
            if (рендеры == null || рендеры.Length == 0) yield break;
            
            объектСейчасВиден = true;
            yield return StartCoroutine(ПлавнаяАльфа(рендеры, 0f, 1f, скоростьАнимации));
            
            if (показыватьСообщения)
                Debug.Log($"[MiniGame] Объект {текущийИндекс + 1} ПОЯВИЛСЯ! Жми E!");
            
            float таймер = 0f;
            while (таймер < интервалМигания && миниИграАктивна && текущийИндекс < объектыДляЦикла.Length && !ожидаемСледующийЦикл)
            {
                таймер += Time.deltaTime;
                yield return null;
            }
            
            if (ожидаемСледующийЦикл) break;
            if (!миниИграАктивна) break;
            if (текущийИндекс >= объектыДляЦикла.Length) break;
            
            объектСейчасВиден = false;
            yield return StartCoroutine(ПлавнаяАльфа(рендеры, 1f, 0f, скоростьАнимации));
            
            if (показыватьСообщения)
                Debug.Log($"[MiniGame] Объект {текущийИндекс + 1} ИСЧЕЗ");
            
            таймер = 0f;
            while (таймер < интервалМигания && миниИграАктивна && текущийИндекс < объектыДляЦикла.Length && !ожидаемСледующийЦикл)
            {
                таймер += Time.deltaTime;
                yield return null;
            }
            
            if (ожидаемСледующийЦикл) break;
        }
    }

    IEnumerator ПлавнаяАльфа(SpriteRenderer[] рендеры, float от, float до, float длительность)
    {
        float время = 0f;
        Color[] начальныеЦвета = new Color[рендеры.Length];
        
        for (int i = 0; i < рендеры.Length; i++)
            if (рендеры[i] != null)
                начальныеЦвета[i] = рендеры[i].color;
        
        while (время < длительность)
        {
            время += Time.deltaTime;
            float t = время / длительность;
            float альфа = Mathf.Lerp(от, до, t);
            
            for (int i = 0; i < рендеры.Length; i++)
            {
                if (рендеры[i] != null)
                {
                    Color цв = начальныеЦвета[i];
                    цв.a = альфа;
                    рендеры[i].color = цв;
                }
            }
            yield return null;
        }
        
        SetAlpha(рендеры, до);
    }

    void SetAlpha(SpriteRenderer[] рендеры, float альфа)
    {
        foreach (var r in рендеры)
        {
            if (r != null)
            {
                Color цв = r.color;
                цв.a = альфа;
                r.color = цв;
            }
        }
    }
}