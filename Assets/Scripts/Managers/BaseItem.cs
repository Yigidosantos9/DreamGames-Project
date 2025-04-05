using UnityEngine;
// A BaseItem class that contains shared properties for all items (cubes, obstacles, etc.)
public class BaseItem : MonoBehaviour
{
    protected BoardManager boardManager;
    public int x;
    public int y;

    // Init will be called by BoardManager
    public virtual void Init(BoardManager board, int gridX, int gridY)
    {
        boardManager = board;
        x = gridX;
        y = gridY;
    }

    public virtual bool CanFall()
    {
        // By default, items do not fall
        return false;
    }

    public virtual void TakeDamage()
    {
        // By default, item is destroyed on a single hit
        Debug.Log($"TakeDamage => RemoveFromBoard at {x},{y}");
        RemoveFromBoard();
    }
    // When a cube is destroyed, it should also be removed from the BoardManager array
    public virtual void RemoveFromBoard()
    {
        boardManager.items[x, y] = null;
        Destroy(gameObject);
    }
}