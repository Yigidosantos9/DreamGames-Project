using UnityEngine;

public class VaseObstacle : BaseItem
{
    public int health = 2;
    public Sprite damagedSprite;   // broken vase
    private SpriteRenderer spriteRenderer;

    public override void Init(BoardManager board, int gridX, int gridY)
    {
        base.Init(board, gridX, gridY);
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // they can fall
    public override bool CanFall()
    {
        return true;
    }
    // they have 2 health 
    public override void TakeDamage()
    {
        health--;

        if (health <= 0)
        {
            RemoveFromBoard();
        }
        else
        {
            if (damagedSprite != null)
            {
                spriteRenderer.sprite = damagedSprite;
            }
            Debug.Log("Vase damaged, remaining health: " + health);
        }
    }
}