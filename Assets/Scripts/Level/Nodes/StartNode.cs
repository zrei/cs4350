using UnityEngine;

public class StartNode : NodeInternal
{
    public override void Initialise()
    {
        // Cleared by default
        ClearNode();
    }

    public override void OnEnterNode()
    {
        Debug.Log("Entered Start Node");
    }
    
    public override void OnClearNode()
    {
    }
    
    public override void OnExitNode()
    {
        Debug.Log("Exited Start Node");
    }
}
