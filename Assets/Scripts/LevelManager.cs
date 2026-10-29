using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    [Header("Настройки перехода")]
    [SerializeField] private float loadDelay = 0.1f;
    
    private static LevelManager instance;
    private ScreenFader screenFader;
    private MenuManager menuManager;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[LevelManager] Синглтон создан!");
        }
        else
        {
            Debug.Log("[LevelManager] Уничтожаю дубликат!");
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // ===== ИЩЕМ ИЛИ СОЗДАЁМ ScreenFader =====
        НайтиИлиСоздатьScreenFader();
        // ========================================
        
        menuManager = FindFirstObjectByType<MenuManager>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void НайтиИлиСоздатьScreenFader()
    {
        screenFader = FindFirstObjectByType<ScreenFader>();
        
        if (screenFader == null)
        {
            GameObject faderObj = new GameObject("ScreenFader");
            screenFader = faderObj.AddComponent<ScreenFader>();
            DontDestroyOnLoad(faderObj);
            Debug.Log("[LevelManager] ScreenFader создан принудительно!");
        }
        else
        {
            Debug.Log("[LevelManager] ScreenFader найден!");
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ===== ПРОВЕРЯЕМ ScreenFader ПОСЛЕ ЗАГРУЗКИ СЦЕНЫ =====
        if (screenFader == null)
        {
            НайтиИлиСоздатьScreenFader();
        }
        // ========================================================

        if (screenFader != null)
        {
            screenFader.RefreshPlayerReference();
            screenFader.EndTransition();
            Debug.Log($"[LevelManager] Затемнение завершено на сцене {scene.name}");
        }
        else
        {
            Debug.LogError($"[LevelManager] ScreenFader не найден на сцене {scene.name}!");
        }
    }

    public void LoadNextLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;
        
        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            StartCoroutine(LoadSceneWithFullFade(nextIndex));
        }
        else
        {
            Debug.Log("Поздравляем! Вы прошли игру!");
        }
    }

    public void LoadScene(int sceneIndex)
    {
        StartCoroutine(LoadSceneWithFullFade(sceneIndex));
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneWithFullFade(sceneName));
    }

    private IEnumerator LoadSceneWithFullFade(int sceneIndex)
    {
        if (menuManager != null)
        {
            menuManager.OnTransitionStart();
        }
        
        // ===== ПЕРЕД ЗАТЕМНЕНИЕМ ПРОВЕРЯЕМ ScreenFader =====
        if (screenFader == null)
        {
            НайтиИлиСоздатьScreenFader();
        }
        // ====================================================
        
        if (screenFader == null)
        {
            Debug.LogError("ScreenFader не найден! Загружаю без затемнения.");
            SceneManager.LoadScene(sceneIndex);
            yield break;
        }
        
        bool transitionCompleted = false;
        
        screenFader.StartTransition(() => {
            transitionCompleted = true;
        });
        
        while (!transitionCompleted)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(loadDelay);
        
        SceneManager.LoadScene(sceneIndex);
    }

    private IEnumerator LoadSceneWithFullFade(string sceneName)
    {
        if (menuManager != null)
        {
            menuManager.OnTransitionStart();
        }
        
        // ===== ПЕРЕД ЗАТЕМНЕНИЕМ ПРОВЕРЯЕМ ScreenFader =====
        if (screenFader == null)
        {
            НайтиИлиСоздатьScreenFader();
        }
        // ====================================================
        
        if (screenFader == null)
        {
            Debug.LogError("ScreenFader не найден! Загружаю без затемнения.");
            SceneManager.LoadScene(sceneName);
            yield break;
        }
        
        bool transitionCompleted = false;
        
        screenFader.StartTransition(() => {
            transitionCompleted = true;
        });
        
        while (!transitionCompleted)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(loadDelay);
        
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentLevel()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        LoadScene(currentIndex);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}