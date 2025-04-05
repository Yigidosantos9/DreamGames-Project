using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int currentLevel;
    public int maxLevel = 10;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // opens the last level played
        currentLevel = PlayerPrefs.GetInt("LastPlayedLevel", 1);
    }

    public void SaveLevel()
    {
        PlayerPrefs.SetInt("LastPlayedLevel", currentLevel);
        PlayerPrefs.Save();
    }
}