using UnityEngine;

public class BoxObstacle : BaseItem
{
    // box do not fall, they are removed with one damage
    public override bool CanFall()
    {
        return false;
    }
    public override void TakeDamage()
    {
        // they are removed with one damage
        RemoveFromBoard();
    }
}