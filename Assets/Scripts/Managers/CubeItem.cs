using UnityEngine;
public class CubeItem : BaseItem
{
    public string color;
    public override void Init(BoardManager board, int gridX, int gridY)
    {
        base.Init(board, gridX, gridY);
    }

    void OnMouseDown()
    {
        Debug.Log("Cube clicked at: " + x + ", " + y);
        boardManager.OnCubeClicked(this);
    }
}