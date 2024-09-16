using UnityEngine;

public class BattleNode : NodeInternal
{
    public override void OnEnterNode()
    {
        Debug.Log("Entered Battle Node");
    }
    
    public override void OnClearNode()
    {
        Debug.Log("Cleared Battle Node");
        ClearNode();
    }
    
    public override void OnExitNode()
    {
        Debug.Log("Exited Battle Node");
    }
}
