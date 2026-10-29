using UnityEngine;

public class GameProgress : MonoBehaviour
{
    private static int highestLevelUnlocked = 0;
    
    void Start()
    {
        LoadProgress();
    }
    
    public void UnlockLevel(int levelIndex)
    {
        if (levelIndex > highestLevelUnlocked)
        {
            highestLevelUnlocked = levelIndex;
            SaveProgress();
        }
    }
    
    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex <= highestLevelUnlocked;
    }
    
    void SaveProgress()
    {
        PlayerPrefs.SetInt("HighestLevel", highestLevelUnlocked);
        PlayerPrefs.Save();
    }
    
    void LoadProgress()
    {
        highestLevelUnlocked = PlayerPrefs.GetInt("HighestLevel", 0);
    }
    
    public void ResetProgress()
    {
        highestLevelUnlocked = 0;
        SaveProgress();
    }
}