using UnityEngine;
using System.Collections;

public class FrogSequence : MonoBehaviour
{
    [Header("Точки прыжков")]
    public Transform точкаНаЛампе;
    public Transform точкаНаТумбочке;
    public Transform точкаНаПолу;

    [Header("Игрок")]
    public GameObject игрок;

    [Header("Язык лягушки")]
    public GameObject языкЛягушки;
    public float длительностьАнимацииЯзыка = 0.5f;

    [Header("=== НАСТРОЙКА ПОЗИЦИИ ЯЗЫКА ===")]
    public Transform языкПустышка;

    [Header("=== НАСТРОЙКИ РАСТЯЖЕНИЯ ЯЗЫКА ===")]
    public bool растягиватьЯзык = true;
    public float минимальнаяДлинаЯзыка = 0.5f;
    public float максимальнаяДлинаЯзыка = 4f;
    public AnimationCurve криваяРастяжения = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("=== ВИЗУАЛЬНЫЕ ПОМОЩНИКИ ===")]
    public bool показыватьПомощников = true;
    public float радиусПомощника = 0.3f;

    [Header("Настройки прыжка")]
    public float длительностьПрыжка = 0.5f;
    public float высотаДуги = 2f;
    public AnimationCurve криваяДуги = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Настройки исчезания игрока")]
    public float скоростьИсчезанияИгрока = 0.3f;

    [Header("Настройки анимаций")]
    public string анимацияПрыжка = "Jump";
    public string анимацияЕды = "Eat";
    public string анимацияСчастливый = "Froog Happy";
    public float длительностьСчастливого = 1.5f;
    public float задержкаПослеЕды = 0.5f;

    [Header("Переход на следующий уровень")]
    public string имяСледующегоУровня = "Level_3.1";
    public float задержкаПередПереходом = 1f;

    private Animator animator;
    private bool sequenceStarted = false;
    private LevelManager levelManager;
    private Animator языкAnimator;
    private bool игрокИсчез = false;
    private SpriteRenderer языкSpriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        
        levelManager = FindFirstObjectByType<LevelManager>();
        if (levelManager == null)
        {
            Debug.LogWarning("[FrogSequence] LevelManager не найден!");
        }
        
        if (точкаНаЛампе != null)
            transform.position = точкаНаЛампе.position;

        if (языкЛягушки != null)
        {
            языкAnimator = языкЛягушки.GetComponent<Animator>();
            языкSpriteRenderer = языкЛягушки.GetComponent<SpriteRenderer>();
            языкЛягушки.SetActive(false);
        }

        if (языкПустышка == null)
        {
            GameObject pivot = new GameObject("FrogTonguePivot");
            pivot.transform.SetParent(transform);
            pivot.transform.localPosition = new Vector3(0.5f, -0.5f, 0);
            pivot.transform.localRotation = Quaternion.identity;
            языкПустышка = pivot.transform;
            Debug.Log("[FrogSequence] Создана автоматическая пустышка для языка");
        }
    }

    void Update()
    {
        #if UNITY_EDITOR
        if (!Application.isPlaying && языкПустышка != null && языкПустышка.parent != transform)
        {
            языкПустышка.SetParent(transform);
            языкПустышка.localPosition = new Vector3(0.5f, -0.5f, 0);
            языкПустышка.localRotation = Quaternion.identity;
        }
        #endif
    }

    public void НачатьПоследовательность()
    {
        if (sequenceStarted) return;
        sequenceStarted = true;

        // ===== ВЫЗЫВАЕМ ПОДСКАЗКУ =====
        TaksaManager taksa = FindFirstObjectByType<TaksaManager>();
        if (taksa != null)
        {
            taksa.НаFrogSequence();
        }
        // ===============================

        StartCoroutine(ВыполнитьПоследовательность());
    }

    IEnumerator ВыполнитьПоследовательность()
    {
        Debug.Log("[Frog] Froog Happy! Лягушка радуется!");
        
        if (animator != null && !string.IsNullOrEmpty(анимацияСчастливый))
        {
            animator.Play(анимацияСчастливый);
        }
        
        yield return new WaitForSeconds(длительностьСчастливого);

        Debug.Log("[Frog] Показываю язык с растяжением!");
        ПоказатьЯзык();

        yield return new WaitForSeconds(длительностьАнимацииЯзыка);

        Debug.Log("[Frog] Игрок исчезает!");
        
        СкрытьЯзык();
        
        if (игрок != null && !игрокИсчез)
        {
            yield return StartCoroutine(ПлавноИсчезнуть(игрок, скоростьИсчезанияИгрока));
            игрокИсчез = true;
        }

        yield return new WaitForSeconds(задержкаПослеЕды);

        Debug.Log("[Frog] Прыгаю на тумбочку!");
        
        if (animator != null)
            animator.Play(анимацияПрыжка);

        yield return StartCoroutine(ПрыгнутьПоДуге(точкаНаТумбочке.position, длительностьПрыжка));
        yield return new WaitForSeconds(0.3f);

        Debug.Log("[Frog] Прыгаю на пол!");
        
        yield return StartCoroutine(ПрыгнутьПоДуге(точкаНаПолу.position, длительностьПрыжка * 1.5f));

        yield return new WaitForSeconds(задержкаПередПереходом);

        Debug.Log($"[Frog] Переход на уровень {имяСледующегоУровня}");
        
        if (levelManager != null)
        {
            levelManager.LoadScene(имяСледующегоУровня);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(имяСледующегоУровня);
        }
    }

    void ПоказатьЯзык()
    {
        if (языкЛягушки == null || языкПустышка == null) return;

        языкЛягушки.transform.position = языкПустышка.position;
        языкЛягушки.transform.rotation = языкПустышка.rotation;
        
        if (растягиватьЯзык && игрок != null && игрок.activeSelf)
        {
            float расстояние = Vector3.Distance(языкПустышка.position, игрок.transform.position);
            float масштаб = Mathf.Clamp(расстояние / 1f, минимальнаяДлинаЯзыка, максимальнаяДлинаЯзыка);
            
            float t = Mathf.Clamp01((масштаб - минимальнаяДлинаЯзыка) / (максимальнаяДлинаЯзыка - минимальнаяДлинаЯзыка));
            float кривая = криваяРастяжения.Evaluate(t);
            float итоговыйМасштаб = Mathf.Lerp(минимальнаяДлинаЯзыка, максимальнаяДлинаЯзыка, кривая);
            
            языкЛягушки.transform.localScale = new Vector3(
                языкПустышка.localScale.x,
                итоговыйМасштаб,
                языкПустышка.localScale.z
            );
            
            Debug.Log($"[FrogSequence] Язык растянут по Y: расстояние={расстояние:F2}, масштаб={итоговыйМасштаб:F2}");
        }
        else
        {
            языкЛягушки.transform.localScale = языкПустышка.localScale;
        }
        
        if (языкSpriteRenderer != null)
        {
            языкSpriteRenderer.sortingOrder = 100;
        }
        
        языкЛягушки.SetActive(true);

        if (языкAnimator != null)
        {
            языкAnimator.Play("TongueAttack");
        }

        Debug.Log("[FrogSequence] Язык показан!");
    }

    void СкрытьЯзык()
    {
        if (языкЛягушки != null)
        {
            языкЛягушки.SetActive(false);
        }
    }

    IEnumerator ПрыгнутьПоДуге(Vector3 цель, float длительность)
    {
        Vector3 старт = transform.position;
        float время = 0f;

        while (время < длительность)
        {
            время += Time.deltaTime;
            float t = время / длительность;
            float кривая = криваяДуги.Evaluate(t);

            Vector3 текущаяПозиция = Vector3.Lerp(старт, цель, кривая);
            float высота = Mathf.Sin(t * Mathf.PI) * высотаДуги;
            текущаяПозиция.y += высота;

            transform.position = текущаяПозиция;

            yield return null;
        }

        transform.position = цель;
    }

    IEnumerator ПлавноИсчезнуть(GameObject obj, float длительность)
    {
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        Color цвет = sr.color;
        float время = 0f;

        while (время < длительность)
        {
            время += Time.deltaTime;
            float t = время / длительность;
            цвет.a = Mathf.Lerp(1f, 0f, t);
            sr.color = цвет;
            yield return null;
        }

        цвет.a = 0f;
        sr.color = цвет;
        obj.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        if (!показыватьПомощников) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
        
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.5f, "🐸 ЛЯГУШКА");
        #endif

        if (языкПустышка != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(языкПустышка.position, радиусПомощника);
            
            Gizmos.color = Color.yellow;
            Vector3 направление = языкПустышка.right * 0.8f;
            Gizmos.DrawRay(языкПустышка.position, направление);
            
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, языкПустышка.position);

            Gizmos.color = Color.white;
            float size = 0.15f;
            Gizmos.DrawLine(языкПустышка.position - Vector3.right * size, языкПустышка.position + Vector3.right * size);
            Gizmos.DrawLine(языкПустышка.position - Vector3.up * size, языкПустышка.position + Vector3.up * size);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(языкПустышка.position + Vector3.up * 0.7f, 
                $"👅 ЯЗЫК\nЛок. позиция: ({языкПустышка.localPosition.x:F2}; {языкПустышка.localPosition.y:F2})\nУгол: {языкПустышка.localEulerAngles.z:F0}°");
            
            if (растягиватьЯзык && игрок != null)
            {
                float расстояние = Vector3.Distance(языкПустышка.position, игрок.transform.position);
                UnityEditor.Handles.Label(языкПустышка.position + Vector3.down * 0.5f, 
                    $"📏 Расстояние до игрока: {расстояние:F2}\n⚡ Язык растянется по Y!");
            }
            #endif
        }
    }

    void OnDrawGizmos()
    {
        if (!показыватьПомощников || Application.isPlaying) return;

        if (языкПустышка != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(языкПустышка.position, 0.2f);
        }
    }
}