using UnityEngine;

public class RocketItem : BaseItem
{
    public bool isVertical;
    public Sprite verticalSprite;
    public Sprite horizontalSprite;

    private SpriteRenderer spriteRenderer;

    public override bool CanFall() => true;

    public override void Init(BoardManager board, int gridX, int gridY)
    {
        base.Init(board, gridX, gridY);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetDirection(bool vertical)
    {
        isVertical = vertical;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = isVertical ? verticalSprite : horizontalSprite;
    }

    void OnMouseDown()
    {
        if (boardManager.levelManager != null &&
            (boardManager.levelManager.isGameOver || boardManager.levelManager.movesLeft <= 0))
        {
            Debug.Log("No moves left - rocket cannot be triggered.");
            return;
        }
        boardManager.levelManager?.OnValidTap();
        Explode();
    }

    private RocketItem CheckAdjacentRocket()
    {
        if (y < boardManager.grid_height - 1)
        {
            BaseItem upItem = boardManager.items[x, y + 1];
            if (upItem is RocketItem upRocket && upRocket != this)
                return upRocket;
        }
        if (y > 0)
        {
            BaseItem downItem = boardManager.items[x, y - 1];
            if (downItem is RocketItem downRocket && downRocket != this)
                return downRocket;
        }
        if (x < boardManager.grid_width - 1)
        {
            BaseItem rightItem = boardManager.items[x + 1, y];
            if (rightItem is RocketItem rightRocket && rightRocket != this)
                return rightRocket;
        }
        if (x > 0)
        {
            BaseItem leftItem = boardManager.items[x - 1, y];
            if (leftItem is RocketItem leftRocket && leftRocket != this)
                return leftRocket;
        }
        return null;
    }
    private void ExplodeSingle()
    {
        if (isVertical)
        {
            for (int py = 0; py < boardManager.grid_height; py++)
            {
                if (boardManager.items[x, py] != null)
                    boardManager.items[x, py].TakeDamage();
            }
        }
        else
        {
            for (int px = 0; px < boardManager.grid_width; px++)
            {
                if (boardManager.items[px, y] != null)
                    boardManager.items[px, y].TakeDamage();
            }
        }
        RemoveFromBoard();
        boardManager.MakeItemsFall();
        boardManager.RefillBoard();
        boardManager.UpdateUI();
        if (boardManager.AreAllObstaclesCleared())
            boardManager.levelManager.Win();
    }

    private void ExplodeLargePlus(int cx, int cy)
    {
        for (int col = cx - 1; col <= cx + 1; col++)
        {
            if (col < 0 || col >= boardManager.grid_width)
                continue;
            for (int row = 0; row < boardManager.grid_height; row++)
            {
                DamageCell(col, row);
            }
        }
        for (int row = cy - 1; row <= cy + 1; row++)
        {
            if (row < 0 || row >= boardManager.grid_height)
                continue;
            for (int col = 0; col < boardManager.grid_width; col++)
            {
                DamageCell(col, row);
            }
        }
    }

    private void DamageCell(int tx, int ty)
    {
        if (tx >= 0 && tx < boardManager.grid_width &&
            ty >= 0 && ty < boardManager.grid_height)
        {
            if (boardManager.items[tx, ty] != null)
                boardManager.items[tx, ty].TakeDamage();
        }
    }
    private void ExplodeCombo(RocketItem adjacentRocket)
    {
        Debug.Log("Rocket Combo triggered!");

        RemoveFromBoard();
        adjacentRocket.RemoveFromBoard();

        ExplodeLargePlus(x, y);

        boardManager.MakeItemsFall();
        boardManager.RefillBoard();
        boardManager.UpdateUI();
        if (boardManager.AreAllObstaclesCleared())
            boardManager.levelManager.Win();
    }

    public void Explode()
    {
        Debug.Log($"Rocket Explode! vertical={isVertical}");
        RocketItem adjacentRocket = CheckAdjacentRocket();
        if (adjacentRocket != null)
        {
            ExplodeCombo(adjacentRocket);
        }
        else
        {
            ExplodeSingle();
        }
    }
}