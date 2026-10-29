using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Панели меню")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject aboutPanel;

    private static MenuManager instance;
    private bool isMenuOpen = false;
    private bool isFirstLaunch = true;
    private bool isGameActive = false;
    private bool isTransitioning = false;  // ← НОВАЯ ПЕРЕМЕННАЯ (идёт переход)

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        if (isFirstLaunch)
        {
            ShowMainMenu();
            Time.timeScale = 0f;
            isMenuOpen = true;
            isGameActive = false;
            isFirstLaunch = false;
        }
        else
        {
            HideAllPanels();
            Time.timeScale = 1f;
            isMenuOpen = false;
            isGameActive = true;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Переход закончился - можно снова открывать меню
        isTransitioning = false;
        
        if (scene.name.Contains("Level") || scene.name == "SampleScene")
        {
            HideAllPanels();
            Time.timeScale = 1f;
            isMenuOpen = false;
            isGameActive = true;
            
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
        }
    }

    void Update()
    {
        // ★ ГЛАВНОЕ: если идёт переход - меню НЕ открывается ★
        if (isTransitioning)
        {
            return;  // Игнорируем ESC во время перехода
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC нажата! isGameActive=" + isGameActive + ", isMenuOpen=" + isMenuOpen);
            
            if (isGameActive)
            {
                if (isMenuOpen)
                {
                    CloseMenu();
                }
                else
                {
                    OpenMenu();
                }
            }
        }
    }

    // ★ НОВЫЙ МЕТОД: вызывается LevelManager перед переходом ★
    public void OnTransitionStart()
    {
        Debug.Log("Меню: начался переход, блокируем ESC");
        isTransitioning = true;
        
        // Если меню было открыто - закрываем его
        if (isMenuOpen)
        {
            CloseMenu();
        }
    }

    public void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    public void ShowSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    public void ShowAbout()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(true);
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(true);
        isMenuOpen = false;
        isGameActive = true;
        HideAllPanels();
    }

    public void ResumeGame()
    {
        CloseMenu();
    }

    public void QuitGame()
    {
        Debug.Log("Выход из игры");
        Application.Quit();
    }

    private void OpenMenu()
    {
        Debug.Log("Открываем меню паузы");
        ShowMainMenu();
        Time.timeScale = 0f;
        isMenuOpen = true;
        isGameActive = true;
    }

    private void CloseMenu()
    {
        Debug.Log("Закрываем меню паузы");
        HideAllPanels();
        Time.timeScale = 1f;
        isMenuOpen = false;
        isGameActive = true;
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (aboutPanel != null) aboutPanel.SetActive(false);
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}