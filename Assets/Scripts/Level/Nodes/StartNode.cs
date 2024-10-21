using UnityEngine;

public class StartNode : LevelNodeInternal
{
    public override void Initialise()
    {
        // Cleared by default
        SetCleared();
    }
    
    public override void ClearNode()
    {
    }
}
