using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainSceneUI : MonoBehaviour
{
    public TMP_Text levelButtonText;
    public Button levelButton;

    void Start()
    {
        int current = GameManager.Instance.currentLevel;
        int max = GameManager.Instance.maxLevel;

        if (current > max)
        {
            levelButtonText.text = "Finished";
            levelButton.interactable = false;
        }
        else
        {
            levelButtonText.text = "Level " + current;
            levelButton.interactable = true;
        }
    }
    public void OnLevelButtonClicked()
    {
        Debug.Log("Clicked Level Button! Loading LevelScene...");
        SceneManager.LoadScene("LevelScene");
    }
}