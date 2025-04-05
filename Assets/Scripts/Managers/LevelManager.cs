using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
public class LevelManager : MonoBehaviour
{
    public int movesLeft = 20;
    public BoardManager boardManager;
    public TopBarUI topBarUI;
    public bool isGameOver = false;
    public GameObject failPopup;
    void Start()
    {
        if (boardManager == null)
        {
            boardManager = FindObjectOfType<BoardManager>();
        }

        LoadLevelData(GameManager.Instance.currentLevel);
        boardManager.UpdateUI();
    }

    public void LoadLevelData(int levelNumber)
    {
        string filePath = Path.Combine(Application.dataPath, "Levels", "level_" + levelNumber + ".json");

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            LevelData levelData = JsonUtility.FromJson<LevelData>(dataAsJson);

            movesLeft = levelData.move_count;
            boardManager.grid_width = levelData.grid_width;
            boardManager.grid_height = levelData.grid_height;

            boardManager.SetupBoardFromData(levelData.grid);
            boardManager.AdjustCamera(boardManager.grid_width, boardManager.grid_height);

            Debug.Log("Level " + levelNumber + " loaded: movesLeft = " + movesLeft);
        }
        else
        {
            Debug.LogError("Level file not found at: " + filePath);
        }
    }

    public void OnValidTap()
    {
        movesLeft--;
        Debug.Log("Moves left: " + movesLeft);

        if (movesLeft <= 0)
        {
            if (AreAllObstaclesCleared())
            {
                Win();
            }
            else
            {
                Fail();
            }
        }
        boardManager.UpdateUI();
    }

    bool AreAllObstaclesCleared()
    {
        return boardManager.AreAllObstaclesCleared();
    }

    public void Win()
    {
        isGameOver = true;
        Debug.Log("Level Won!");
        GameManager.Instance.currentLevel++;
        GameManager.Instance.SaveLevel();
        SceneManager.LoadScene("MainScene");
    }

    public void Fail()
    {
        isGameOver = true;
        Debug.Log("Level Failed!");
        // close topbar
        if (boardManager != null && boardManager.topBarUI != null)
        {
            boardManager.topBarUI.gameObject.SetActive(false);
        }
        // show fail pop-up
        if (failPopup != null)
        {
            failPopup.SetActive(true);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void OnFailCloseButton()
    {
        // go back to mainscene
        SceneManager.LoadScene("MainScene");
    }

    public void OnFailTryAgainButton()
    {
        // play the same level
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}