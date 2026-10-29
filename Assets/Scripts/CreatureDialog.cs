using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CreatureDialog : MonoBehaviour
{
    [Header("=== НАСТРОЙКИ ПОВЕДЕНИЯ ===")]
    public float времяПоказа = 3f;
    public bool требуетНажатияE = false;
    public string состояниеID = "default";
    public string требуетсяСостояние = "";

    [Header("=== ВЫБОР СЦЕНЫ ===")]
    public SceneName текущаяСцена = SceneName.Level_3;

    public enum SceneName
    {
        Level_1,
        Level_2,
        Level_3,
        Level_3_1,
        Level_4
    }

    [Header("=== РЕПЛИКИ ДЛЯ КАЖДОЙ СЦЕНЫ ===")]
    public SceneDialogs диалогиНа1Сцене;
    public SceneDialogs диалогиНа2Сцене;
    public SceneDialogs диалогиНа3Сцене;
    public SceneDialogs диалогиНа3_1Сцене;
    public SceneDialogs диалогиНа4Сцене;

    [Header("=== НАСТРОЙКИ ТЕКСТА ===")]
    public Color цветТекста = Color.white;
    public float размерШрифта = 36;
    public Vector3 смещениеТекста = new Vector3(0, 2f, 0);

    [System.Serializable]
    public class SceneDialogs
    {
        [Header("Основная реплика")]
        public string основнаяРеплика = "Привет!";

        [Header("Реплики после событий")]
        public string репликаПослеОткрытияДвери = "Молодец! Дверь открыта!";
        public string репликаПослеПолученияСока = "Спасибо за сок!";
        public string репликаПослеОтдачиРычага = "Отлично! Ты получил рычаг!";
        public string репликаПослеПревращения = "Теперь ты рычаг!";
        public string репликаПослеПобеды = "Поздравляю!";
    }

    private TextMeshProUGUI текстРеплики;
    private GameObject текстОбъект;
    private bool игрокРядом = false;
    private bool репликаПоказана = false;
    private Coroutine корутинаРеплики;
    private string текущаяРеплика;
    private bool диалогАктивен = false;
    private SceneDialogs текущиеДиалоги;

    void Start()
    {
        Debug.Log($"[CreatureDialog] Start на объекте {gameObject.name}");

        // ===== СОЗДАЁМ ТЕКСТ ПРОГРАММНО =====
        СоздатьТекст();
        // ====================================

        switch (текущаяСцена)
        {
            case SceneName.Level_1:
                текущиеДиалоги = диалогиНа1Сцене;
                break;
            case SceneName.Level_2:
                текущиеДиалоги = диалогиНа2Сцене;
                break;
            case SceneName.Level_3:
                текущиеДиалоги = диалогиНа3Сцене;
                break;
            case SceneName.Level_3_1:
                текущиеДиалоги = диалогиНа3_1Сцене;
                break;
            case SceneName.Level_4:
                текущиеДиалоги = диалогиНа4Сцене;
                break;
            default:
                текущиеДиалоги = диалогиНа3Сцене;
                break;
        }

        if (текущиеДиалоги != null)
        {
            текущаяРеплика = текущиеДиалоги.основнаяРеплика;
            Debug.Log($"[CreatureDialog] Текущая реплика: {текущаяРеплика}");
        }
        else
        {
            Debug.LogError("[CreatureDialog] текущиеДиалоги = null! Проверь настройки в инспекторе!");
        }
    }

    void СоздатьТекст()
    {
        // Создаём объект для текста
        текстОбъект = new GameObject("Реплика");
        текстОбъект.transform.SetParent(transform);
        текстОбъект.transform.localPosition = смещениеТекста;
        текстОбъект.transform.localRotation = Quaternion.identity;
        текстОбъект.transform.localScale = Vector3.one;

        // Добавляем Canvas для World Space
        Canvas canvas = текстОбъект.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        // Добавляем CanvasScaler
        CanvasScaler scaler = текстОбъект.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        // Добавляем текст
        текстРеплики = текстОбъект.AddComponent<TextMeshProUGUI>();
        текстРеплики.text = " ";
        текстРеплики.fontSize = размерШрифта;
        текстРеплики.color = цветТекста;
        текстРеплики.alignment = TextAlignmentOptions.Center;
        текстРеплики.raycastTarget = false;

        // Настраиваем RectTransform
        RectTransform rect = текстРеплики.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 100);
        rect.anchoredPosition = Vector2.zero;

        // Выключаем текст
        текстОбъект.SetActive(false);

        Debug.Log("[CreatureDialog] Текст создан программно!");
    }

    // ===== ОБНОВЛЯЕМ ПОЗИЦИЮ ТЕКСТА ПРИ ИЗМЕНЕНИИ В ИНСПЕКТОРЕ =====
    void OnValidate()
    {
        if (текстОбъект != null && !Application.isPlaying)
        {
            текстОбъект.transform.localPosition = смещениеТекста;
            Debug.Log($"[CreatureDialog] Позиция текста обновлена: {смещениеТекста}");
        }
    }
    // =================================================================

    void Update()
    {
        if (игрокРядом && Input.GetKeyDown(KeyCode.E) && требуетНажатияE && !диалогАктивен)
        {
            Debug.Log("[CreatureDialog] Нажата E! Показываю реплику!");
            ПоказатьРеплику();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[CreatureDialog] OnTriggerEnter2D: {other.name}, тег: {other.tag}");

        if (!other.CompareTag("Player"))
        {
            Debug.Log("[CreatureDialog] Это не игрок, выхожу");
            return;
        }

        игрокРядом = true;
        Debug.Log("[CreatureDialog] Игрок вошёл в зону!");

        if (!string.IsNullOrEmpty(требуетсяСостояние))
        {
            if (!GameState.Get(требуетсяСостояние))
            {
                Debug.Log($"[CreatureDialog] Состояние '{требуетсяСостояние}' не выполнено, реплика не показывается");
                return;
            }
            else
            {
                Debug.Log($"[CreatureDialog] Состояние '{требуетсяСостояние}' выполнено!");
            }
        }

        if (!требуетНажатияE && !диалогАктивен)
        {
            Debug.Log("[CreatureDialog] Показываю реплику (автоматически)");
            ПоказатьРеплику();
        }
        else if (требуетНажатияE)
        {
            Debug.Log("[CreatureDialog] Ожидаю нажатия E");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log("[CreatureDialog] Игрок вышел из зоны");
        игрокРядом = false;
        СкрытьРеплику();
    }

    void ПоказатьРеплику()
    {
        Debug.Log($"[CreatureDialog] ПоказатьРеплику() вызван! Текст: {текущаяРеплика}");

        if (репликаПоказана || диалогАктивен)
        {
            Debug.Log("[CreatureDialog] Реплика уже показана или диалог активен");
            return;
        }

        диалогАктивен = true;
        репликаПоказана = true;

        if (текстРеплики != null)
        {
            Debug.Log($"[CreatureDialog] Устанавливаю текст: {текущаяРеплика}");
            текстРеплики.text = текущаяРеплика;
            текстОбъект.SetActive(true);
        }
        else
        {
            Debug.LogError("[CreatureDialog] текстРеплики = null! Ошибка создания!");
            return;
        }

        if (корутинаРеплики != null)
            StopCoroutine(корутинаРеплики);
        корутинаРеплики = StartCoroutine(СкрытьЧерезВремя());
    }

    void СкрытьРеплику()
    {
        Debug.Log("[CreatureDialog] СкрытьРеплику() вызван");
        репликаПоказана = false;
        диалогАктивен = false;
        if (текстРеплики != null)
        {
            текстРеплики.text = " ";
            текстОбъект.SetActive(false);
        }
    }

    IEnumerator СкрытьЧерезВремя()
    {
        Debug.Log($"[CreatureDialog] СкрытьЧерезВремя() - жду {времяПоказа} секунд");
        yield return new WaitForSeconds(времяПоказа);
        СкрытьРеплику();
    }

    public void ОбновитьРеплику(string новаяРеплика, string новоеСостояние = "")
    {
        Debug.Log($"[CreatureDialog] ОбновитьРеплику: {новаяРеплика}, состояние: {новоеСостояние}");
        текущаяРеплика = новаяРеплика;
        if (!string.IsNullOrEmpty(новоеСостояние))
            состояниеID = новоеСостояние;
    }

    public void ПоказатьРепликуПринудительно(string текст)
    {
        Debug.Log($"[CreatureDialog] ПоказатьРепликуПринудительно: {текст}");
        
        if (корутинаРеплики != null)
            StopCoroutine(корутинаРеплики);

        репликаПоказана = true;
        диалогАктивен = true;

        if (текстРеплики != null)
        {
            текстРеплики.text = текст;
            текстОбъект.SetActive(true);
        }

        корутинаРеплики = StartCoroutine(СкрытьЧерезВремя());
    }

    public void СкрытьРепликуМгновенно()
    {
        Debug.Log("[CreatureDialog] СкрытьРепликуМгновенно()");
        if (корутинаРеплики != null)
            StopCoroutine(корутинаРеплики);
        СкрытьРеплику();
    }

    public void ПослеОткрытияДвери()
    {
        if (текущиеДиалоги != null)
        {
            текущаяРеплика = текущиеДиалоги.репликаПослеОткрытияДвери;
            состояниеID = "door_opened";
            Debug.Log($"[CreatureDialog] ПослеОткрытияДвери: {текущаяРеплика}");
            ПоказатьРеплику();
        }
    }

    public void ПослеПолученияСока()
    {
        if (текущиеДиалоги != null)
        {
            текущаяРеплика = текущиеДиалоги.репликаПослеПолученияСока;
            состояниеID = "has_sok";
            Debug.Log($"[CreatureDialog] ПослеПолученияСока: {текущаяРеплика}");
            ПоказатьРеплику();
        }
    }

    public void ПослеОтдачиРычага()
    {
        if (текущиеДиалоги != null)
        {
            текущаяРеплика = текущиеДиалоги.репликаПослеОтдачиРычага;
            состояниеID = "gave_lever";
            Debug.Log($"[CreatureDialog] ПослеОтдачиРычага: {текущаяРеплика}");
            ПоказатьРеплику();
        }
    }

    public void ПослеПревращения()
    {
        if (текущиеДиалоги != null)
        {
            текущаяРеплика = текущиеДиалоги.репликаПослеПревращения;
            состояниеID = "transformed";
            Debug.Log($"[CreatureDialog] ПослеПревращения: {текущаяРеплика}");
            ПоказатьРеплику();
        }
    }

    public void ПослеПобеды()
    {
        if (текущиеДиалоги != null)
        {
            текущаяРеплика = текущиеДиалоги.репликаПослеПобеды;
            состояниеID = "victory";
            Debug.Log($"[CreatureDialog] ПослеПобеды: {текущаяРеплика}");
            ПоказатьРеплику();
        }
    }
}