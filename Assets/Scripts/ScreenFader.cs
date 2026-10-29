using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 0.5f;
    private Image fadeImage;
    private static ScreenFader instance;
    private PlayerController player;
    private bool isFading = false;
    
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
        }
    }
    
    void Start()
    {
        SetupFadeImage();
        player = FindFirstObjectByType<PlayerController>();
    }
    
    void SetupFadeImage()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
        }
        
        if (fadeImage == null)
        {
            GameObject imageObj = new GameObject("FadeImage");
            imageObj.transform.SetParent(transform);
            fadeImage = imageObj.AddComponent<Image>();
        }
        
        RectTransform rect = fadeImage.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;
    }
    
    public void StartTransition(System.Action onComplete = null)
    {
        if (isFading) return;
        StartCoroutine(TransitionCoroutine(onComplete));
    }
    
    private IEnumerator TransitionCoroutine(System.Action onComplete)
    {
        isFading = true;
        
        BlockPlayer();
        
        yield return StartCoroutine(Fade(0, 1));
        
        yield return null;
        
        yield return new WaitForEndOfFrame();
        
        onComplete?.Invoke();
    }
    
    public void EndTransition()
    {
        if (!isFading) return;
        StartCoroutine(EndTransitionCoroutine());
    }
    
    private IEnumerator EndTransitionCoroutine()
    {
        yield return StartCoroutine(Fade(1, 0));
        
        UnblockPlayer();
        
        isFading = false;
    }
    
    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0;
        SetAlpha(startAlpha);
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            SetAlpha(alpha);
            yield return null;
        }
        
        SetAlpha(endAlpha);
    }
    
    private void BlockPlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        
        if (player != null)
        {
            player.BlockMovement();
            Debug.Log("Игрок ЗАБЛОКИРОВАН");
        }
    }
    
    private void UnblockPlayer()
    {
        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
        
        if (player != null)
        {
            player.UnblockMovement();
            Debug.Log("Игрок РАЗБЛОКИРОВАН");
        }
    }
    
    private void SetAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }
    
    public bool IsFading()
    {
        return isFading;
    }
    
    public void RefreshPlayerReference()
    {
        player = FindFirstObjectByType<PlayerController>();
    }
}