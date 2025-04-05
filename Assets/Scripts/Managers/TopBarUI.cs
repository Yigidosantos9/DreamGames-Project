using UnityEngine;
using TMPro;

public class TopBarUI : MonoBehaviour
{
    public TMP_Text goalText;
    public TMP_Text movesText;

    public void SetGoal(int boxCount, int stoneCount, int vaseCount)
    {
        goalText.text = $"{boxCount}\n{stoneCount}\n{vaseCount}";
    }
    public void SetMoves(int movesLeft)
    {
        movesText.text = $"{movesLeft}";
    }
}