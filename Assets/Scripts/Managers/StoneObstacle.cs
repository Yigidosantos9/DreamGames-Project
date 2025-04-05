using UnityEngine;

public class StoneObstacle : BaseItem
{
    // they do not fall
    public override bool CanFall()
    {
        return false;
    }
    // only takes damage from rockets
    public override void TakeDamage()
    {
        RemoveFromBoard();
    }
}