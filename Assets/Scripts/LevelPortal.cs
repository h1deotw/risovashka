using UnityEngine;

public class LevelPortal : MonoBehaviour
{
    [Header("Куда ведёт портал")]
    [SerializeField] private string levelToLoad = "Level_2";
    
    [Header("Настройки")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool requireInteraction = true;
    [SerializeField] private bool переходОтЛягушки = true;

    [Header("Задержка перед переходом")]
    [SerializeField] private float задержкаПерехода = 0.5f;

    private LevelManager levelManager;
    private bool isPlayerNear = false;
    private bool isFrogNear = false;
    private bool переходАктивирован = false;
    private bool поискЗапущен = false;
    
    void Start()
    {
        НайтиLevelManager();
    }

    void НайтиLevelManager()
    {
        if (поискЗапущен) return;
        поискЗапущен = true;

        levelManager = FindFirstObjectByType<LevelManager>();
        
        if (levelManager == null)
        {
            Debug.LogWarning("[LevelPortal] LevelManager не найден, попробую позже...");
            Invoke(nameof(ПовторитьПоиск), 0.3f);
        }
        else
        {
            Debug.Log("[LevelPortal] LevelManager найден!");
            поискЗапущен = false;
        }
    }

    void ПовторитьПоиск()
    {
        поискЗапущен = false;
        НайтиLevelManager();
    }
    
    void Update()
    {
        if (переходАктивирован) return;
        
        if (requireInteraction && isPlayerNear && Input.GetKeyDown(interactKey))
        {
            АктивироватьПереход("Игрок");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (переходАктивирован) return;
        
        if (other.CompareTag("Player"))
        {
            isPlayerNear = true;
            
            if (!requireInteraction)
            {
                АктивироватьПереход("Игрок (авто)");
            }
            else
            {
                Debug.Log("Нажмите " + interactKey + " для перехода на следующий уровень");
            }
        }
        
        if (переходОтЛягушки)
        {
            try
            {
                if (other.CompareTag("Frog"))
                {
                    isFrogNear = true;
                    Debug.Log("[Portal] Лягушка коснулась портала!");
                    АктивироватьПереход("Лягушка");
                }
            }
            catch (System.Exception)
            {
                Debug.LogWarning("[Portal] Тег 'Frog' не найден в проекте.");
            }
        }
    }
    
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNear = false;
        }
        
        try
        {
            if (other.CompareTag("Frog"))
            {
                isFrogNear = false;
            }
        }
        catch (System.Exception)
        {
            // Игнорируем
        }
    }
    
    void АктивироватьПереход(string источник)
    {
        if (переходАктивирован) return;
        
        переходАктивирован = true;
        Debug.Log($"[Portal] Переход активирован! Источник: {источник}");
        
        Invoke(nameof(Телепорт), задержкаПерехода);
    }
    
    void Телепорт()
    {
        // ===== ПЕРЕД ПЕРЕХОДОМ ПРОВЕРЯЕМ LevelManager =====
        if (levelManager == null)
        {
            levelManager = FindFirstObjectByType<LevelManager>();
            
            if (levelManager == null)
            {
                Debug.LogWarning("[Portal] LevelManager не найден! Загружаю без затемнения.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(levelToLoad);
                return;
            }
        }
        // ====================================================
        
        Debug.Log($"[Portal] Загрузка уровня: {levelToLoad}");
        levelManager.LoadScene(levelToLoad);
    }
}