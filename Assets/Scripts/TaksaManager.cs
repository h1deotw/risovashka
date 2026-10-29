using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class TaksaManager : MonoBehaviour
{
    [Header("Префаб который будет появляться")]
    public GameObject popupPrefab;

    [Header("=== НАСТРОЙКИ ПОЗИЦИИ ===")]
    public Transform точкаСтарт;
    public Transform точкаФиниш;

    [Header("Настройки позиции (в пикселях) - если точки не назначены")]
    public Vector2 отступОтКрая = new Vector2(100, 100);
    public float расстояниеВыезда = 200f;

    [Header("Настройки анимации выезда")]
    public float времяВыезда = 0.5f;
    public AnimationCurve криваяДвижения = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float времяОжиданияВЦентре = 2f;

    [Header("=== НАСТРОЙКИ ТЕКСТА ===")]
    public string текстПодсказки = "ТЕКСТ ПОДСКАЗКИ";
    public float размерШрифта = 36;
    public Color цветТекста = Color.white;
    public TextAlignmentOptions выравнивание = TextAlignmentOptions.Left;
    public TMP_FontAsset шрифт;
    public Vector2 размерОбластиТекста = new Vector2(800, 100);
    public Vector3 смещениеТекста = new Vector3(-1.5f, 0.3f, 0);

    [Header("=== НАСТРОЙКИ ФОНА ===")]
    public bool показыватьФон = true;
    public Color цветФона = new Color(0, 0, 0, 0.8f);
    public Vector2 размерФона = new Vector2(4f, 1.2f);
    public Vector3 смещениеФона = new Vector3(0, 0, 0);

    [Header("=== СОРТИРОВКА ===")]
    public string sortingLayer = "Default";
    public int orderInLayerТекст = 10;
    public int orderInLayerФон = 9;

    [Header("Настройки очереди подсказок")]
    public float задержкаПередПодсказкой = 0.5f;
    public float задержкаПослеПодсказки = 2f;

    [Header("=== ПОДСКАЗКИ ===")]
    public ПодсказкаНастройка[] подсказки;

    private Camera mainCamera;
    private GameObject текущийОбъект;
    private Coroutine текущаяКорутина;
    private GameObject текстоваяГруппа;
    private Queue<Подсказка> очередьПодсказок = new Queue<Подсказка>();
    private bool подсказкаИдет = false;
    private bool[] событияВыполнены;
    private bool ожидаемУбирание = false;
    private bool вечнаяПодсказкаАктивна = false;

    [System.Serializable]
    public class ПодсказкаНастройка
    {
        public string имяСобытия = "Событие";
        [TextArea(2, 4)] public string текст;
        public ТиггерСобытия когдаПоказывать;
        public bool убиратьПосле = true;
        public float задержка = 0f;
        public string связанныйОбъект = "";
    }

    public enum ТиггерСобытия
    {
        Start, OnMove, OnTransform, OnFeatherTransform, OnPressE, OnMinigameWin, OnFrogSequence, OnLevelComplete, Manual
    }

    [System.Serializable]
    public class Подсказка
    {
        public string текст;
        public bool убиратьПосле;
        public float задержка;
    }

    void Start()
    {
        mainCamera = Camera.main;
        Debug.Log("[TaksaManager] ✅ Запущен!");
        int count = System.Enum.GetValues(typeof(ТиггерСобытия)).Length;
        событияВыполнены = new bool[count];
        if (popupPrefab == null) Debug.LogError("[TaksaManager] ❌ popupPrefab не назначен!");
        ПоказатьПодсказкиПоТипу(ТиггерСобытия.Start);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) ПоказатьСТекстом(текстПодсказки);

        // ===== ГЛОБАЛЬНОЕ НАЖАТИЕ E =====
        if (Input.GetKeyDown(KeyCode.E))
        {
            НаГлобальноеНажатиеE();
        }
        // =================================

        if (!событияВыполнены[(int)ТиггерСобытия.OnMove] && Input.GetAxisRaw("Horizontal") != 0)
        {
            string имяСцены = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            if (имяСцены == "Level_1")
            {
                Debug.Log("[TaksaManager] 🚶 Игрок начал ходить на Level_1!");
                ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnMove);
            }
        }
    }

    public void НаГлобальноеНажатиеE()
    {
        if (событияВыполнены[(int)ТиггерСобытия.OnPressE]) return;
        
        // Проверяем, есть ли подсказка с OnPressE
        bool естьПодсказка = false;
        foreach (var настройка in подсказки)
        {
            if (настройка.когдаПоказывать == ТиггерСобытия.OnPressE)
            {
                естьПодсказка = true;
                break;
            }
        }
        
        if (!естьПодсказка) return;
        
        Debug.Log("[TaksaManager] 🔘 Глобальное нажатие E!");
        ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnPressE);
    }

    public void УбратьМгновенно()
    {
        if (текущийОбъект != null)
        {
            if (текущаяКорутина != null)
                StopCoroutine(текущаяКорутина);
            Destroy(текущийОбъект);
            текущийОбъект = null;
            ожидаемУбирание = false;
            вечнаяПодсказкаАктивна = false;
            Debug.Log("[TaksaManager] 🔥 Принудительное убирание таксы!");
        }
    }

    public void НаТрансформацию()
    {
        if (событияВыполнены[(int)ТиггерСобытия.OnTransform]) return;
        ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnTransform);
    }

    public void НаПревращениеВПеро()
    {
        if (событияВыполнены[(int)ТиггерСобытия.OnFeatherTransform]) return;
        ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnFeatherTransform);
    }

    public void НаМиниИгруПобеда()
    {
        if (событияВыполнены[(int)ТиггерСобытия.OnMinigameWin]) return;
        ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnMinigameWin);
    }

    public void НаFrogSequence()
    {
        if (событияВыполнены[(int)ТиггерСобытия.OnFrogSequence]) return;
        ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnFrogSequence);
    }

    public void НаУровеньПройден()
    {
        if (событияВыполнены[(int)ТиггерСобытия.OnLevelComplete]) return;
        ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnLevelComplete);
    }

    public void НаНажатиеE(GameObject объект)
    {
        if (событияВыполнены[(int)ТиггерСобытия.OnPressE]) return;

        foreach (var настройка in подсказки)
        {
            if (настройка.когдаПоказывать == ТиггерСобытия.OnPressE)
            {
                if (string.IsNullOrEmpty(настройка.связанныйОбъект) || 
                    настройка.связанныйОбъект == объект.name || 
                    объект.name.Contains(настройка.связанныйОбъект))
                {
                    ПоказатьПодсказкиПоТипу(ТиггерСобытия.OnPressE);
                    return;
                }
            }
        }
    }

    void ПоказатьПодсказкиПоТипу(ТиггерСобытия тип)
    {
        if (событияВыполнены[(int)тип]) return;
        событияВыполнены[(int)тип] = true;

        // ===== ЕСЛИ ЕСТЬ СТАРАЯ ПОДСКАЗКА — УБИРАЕМ С АНИМАЦИЕЙ =====
        if (текущийОбъект != null)
        {
            if (текущаяКорутина != null)
                StopCoroutine(текущаяКорутина);
            
            StartCoroutine(УбратьОбъект(текущийОбъект));
            вечнаяПодсказкаАктивна = false;
        }
        // ================================================================

        foreach (var настройка in подсказки)
        {
            if (настройка.когдаПоказывать == тип)
            {
                StartCoroutine(ПоказатьСЗадержкой(настройка));
                break;
            }
        }
    }

    IEnumerator ПоказатьСЗадержкой(ПодсказкаНастройка настройка)
    {
        // Ждём, пока старая подсказка уедет
        while (текущийОбъект != null || ожидаемУбирание)
            yield return null;
        
        ПоказатьСТекстом(настройка.текст);
        
        if (настройка.убиратьПосле && настройка.задержка > 0)
        {
            StartCoroutine(УбратьЧерезВремя(настройка.задержка));
        }
    }

    IEnumerator УбратьЧерезВремя(float задержка)
    {
        yield return new WaitForSeconds(задержка);
        
        if (текущийОбъект != null)
        {
            Debug.Log("[TaksaManager] ⏰ Автоматическое убирание подсказки!");
            StartCoroutine(УбратьОбъект(текущийОбъект));
        }
    }

    public void ДобавитьПодсказку(string текст, bool убиратьПосле, float задержка = 0f)
    {
        очередьПодсказок.Enqueue(new Подсказка { 
            текст = текст, 
            убиратьПосле = убиратьПосле, 
            задержка = задержка 
        });
        
        if (!подсказкаИдет) 
            StartCoroutine(ПоказатьСледующуюПодсказку());
    }

    IEnumerator ПоказатьСледующуюПодсказку()
    {
        подсказкаИдет = true;
        
        while (очередьПодсказок.Count > 0)
        {
            while (текущийОбъект != null || ожидаемУбирание) 
                yield return null;

            Подсказка подсказка = очередьПодсказок.Dequeue();
            
            if (подсказка.задержка > 0f) 
                yield return new WaitForSeconds(подсказка.задержка);
            
            ПоказатьСТекстом(подсказка.текст);
            
            if (подсказка.убиратьПосле)
            {
                yield return new WaitForSeconds(задержкаПослеПодсказки);
                Убрать();
            }
            else
            {
                вечнаяПодсказкаАктивна = true;
            }
        }
        
        подсказкаИдет = false;
    }

    public void ПоказатьСТекстом(string текст)
    {
        if (popupPrefab == null) 
        { 
            Debug.LogError("[TaksaManager] popupPrefab не назначен!"); 
            return; 
        }

        Vector3 старт = ПолучитьСтартовуюПозицию();
        Vector3 финиш = ПолучитьЦелевуюПозицию();

        текущийОбъект = Instantiate(popupPrefab, старт, Quaternion.identity);

        текстоваяГруппа = new GameObject("TaksaTextGroup");
        текстоваяГруппа.transform.SetParent(текущийОбъект.transform);
        текстоваяГруппа.transform.localPosition = Vector3.zero;
        текстоваяГруппа.transform.localRotation = Quaternion.identity;
        текстоваяГруппа.transform.localScale = Vector3.one;

        if (показыватьФон)
        {
            GameObject bgGO = new GameObject("TextBackground");
            bgGO.transform.SetParent(текстоваяГруппа.transform);
            bgGO.transform.localPosition = смещениеФона;
            bgGO.transform.localRotation = Quaternion.identity;
            bgGO.transform.localScale = Vector3.one;

            SpriteRenderer bgSR = bgGO.AddComponent<SpriteRenderer>();
            Texture2D tex = new Texture2D(2, 2);
            tex.SetPixel(0, 0, Color.white);
            tex.SetPixel(1, 0, Color.white);
            tex.SetPixel(0, 1, Color.white);
            tex.SetPixel(1, 1, Color.white);
            tex.Apply();

            Sprite bgSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 1);
            bgSR.sprite = bgSprite;
            bgSR.color = цветФона;
            bgSR.sortingLayerName = sortingLayer;
            bgSR.sortingOrder = orderInLayerФон;
            bgGO.transform.localScale = new Vector3(размерФона.x, размерФона.y, 1);
        }

        GameObject canvasGO = new GameObject("TaksaCanvas");
        canvasGO.transform.SetParent(текстоваяГруппа.transform);
        canvasGO.transform.localPosition = смещениеТекста;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one;

        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;
        canvas.sortingLayerName = sortingLayer;
        canvas.sortingOrder = orderInLayerТекст;

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(10, 5);
        canvasRect.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        GameObject textGO = new GameObject("TaksaText");
        textGO.transform.SetParent(canvasGO.transform);
        textGO.transform.localPosition = Vector3.zero;

        TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
        tmp.text = текст;
        tmp.fontSize = размерШрифта;
        tmp.color = цветТекста;
        tmp.alignment = выравнивание;
        tmp.autoSizeTextContainer = false;
        if (шрифт != null) tmp.font = шрифт;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0.5f);
        textRect.anchorMax = new Vector2(0, 0.5f);
        textRect.pivot = new Vector2(0, 0.5f);
        textRect.sizeDelta = размерОбластиТекста;
        textRect.anchoredPosition = Vector2.zero;

        tmp.enableWordWrapping = true;
        tmp.enableAutoSizing = false;

        SpriteRenderer sr = текущийОбъект.GetComponent<SpriteRenderer>();
        if (sr != null) 
        { 
            sr.sortingLayerName = sortingLayer; 
            sr.sortingOrder = 5; 
        }

        текущаяКорутина = StartCoroutine(ВыехатьИОстановиться(текущийОбъект, старт, финиш));
    }

    public void Убрать()
    {
        if (текущийОбъект != null)
        {
            if (текущаяКорутина != null) 
                StopCoroutine(текущаяКорутина);
            
            StartCoroutine(УбратьОбъект(текущийОбъект));
        }
    }

    IEnumerator УбратьОбъект(GameObject obj)
    {
        if (obj == null) yield break;

        ожидаемУбирание = true;
        Vector3 старт = obj.transform.position;
        Vector3 финиш = ПолучитьСтартовуюПозицию();

        float прогресс = 0f;
        while (прогресс < 1f)
        {
            прогресс += Time.deltaTime / времяВыезда;
            прогресс = Mathf.Clamp01(прогресс);
            float t = криваяДвижения.Evaluate(прогресс);
            obj.transform.position = Vector3.Lerp(старт, финиш, t);
            yield return null;
        }

        Destroy(obj);
        текущийОбъект = null;
        ожидаемУбирание = false;
        вечнаяПодсказкаАктивна = false;
        Debug.Log("[TaksaManager] Такса уехала обратно за экран");
    }

    IEnumerator ВыехатьИОстановиться(GameObject obj, Vector3 старт, Vector3 финиш)
    {
        float прогресс = 0f;
        while (прогресс < 1f)
        {
            прогресс += Time.deltaTime / времяВыезда;
            прогресс = Mathf.Clamp01(прогресс);
            float t = криваяДвижения.Evaluate(прогресс);
            obj.transform.position = Vector3.Lerp(старт, финиш, t);
            yield return null;
        }

        obj.transform.position = финиш;
        Debug.Log("[TaksaManager] Такса выехала");
    }

    Vector3 ПолучитьСтартовуюПозицию()
    {
        if (точкаСтарт != null) return точкаСтарт.position;

        Vector3 позицияКамеры = mainCamera.transform.position;
        float высотаКамеры = mainCamera.orthographicSize * 2f;
        float ширинаКамеры = высотаКамеры * mainCamera.aspect;
        float правыйКрай = позицияКамеры.x + (ширинаКамеры / 2f);
        float верхнийКрай = позицияКамеры.y + (высотаКамеры / 2f);
        float отступX = (расстояниеВыезда / Screen.width) * ширинаКамеры;
        float отступY = (расстояниеВыезда / Screen.height) * высотаКамеры;
        return new Vector3(правыйКрай + отступX, верхнийКрай + отступY, позицияКамеры.z + 10);
    }

    Vector3 ПолучитьЦелевуюПозицию()
    {
        if (точкаФиниш != null) return точкаФиниш.position;

        Vector3 позицияКамеры = mainCamera.transform.position;
        float высотаКамеры = mainCamera.orthographicSize * 2f;
        float ширинаКамеры = высотаКамеры * mainCamera.aspect;
        float правыйКрай = позицияКамеры.x + (ширинаКамеры / 2f);
        float верхнийКрай = позицияКамеры.y + (высотаКамеры / 2f);
        float отступX = (отступОтКрая.x / Screen.width) * ширинаКамеры;
        float отступY = (отступОтКрая.y / Screen.height) * высотаКамеры;
        return new Vector3(правыйКрай - отступX, верхнийКрай - отступY, позицияКамеры.z + 10);
    }
}