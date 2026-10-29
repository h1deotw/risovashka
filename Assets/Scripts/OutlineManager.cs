using UnityEngine;

public class OutlineManager : MonoBehaviour
{
    public GameObject targetOutline;  // Сюда перетащишь свой объект с тегом Outline

    void Start()
    {
        Debug.Log("OutlineManager запущен!");
        
        if (targetOutline == null)
        {
            Debug.LogError("targetOutline не назначен! Перетащи объект в инспекторе.");
        }
        else
        {
            // Гарантируем, что объект выключен при старте
            targetOutline.SetActive(false);
            Debug.Log($"Объект {targetOutline.name} выключен при старте");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Debug.Log("LCTRL НАЖАТ! Консоль работает.");
            
            if (targetOutline != null)
            {
                // Включаем объект
                targetOutline.SetActive(true);
                Debug.Log($"Объект {targetOutline.name} ВКЛЮЧЁН");
                
                // Через 2 секунды выключаем
                Invoke(nameof(TurnOffOutline), 2f);
            }
            else
            {
                Debug.LogError("targetOutline = null! Нечего включать.");
            }
        }
    }

    void TurnOffOutline()
    {
        if (targetOutline != null)
        {
            targetOutline.SetActive(false);
            Debug.Log($"Объект {targetOutline.name} ВЫКЛЮЧЁН");
        }
    }
}