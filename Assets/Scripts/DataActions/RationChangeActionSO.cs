using UnityEngine;

[CreateAssetMenu(fileName = "RationChangeActionSO", menuName = "ScriptableObject/DataAction/RationChangeActionSO")]
public class RationChangeActionSO : DataActionSO
{
    public float change;

    public override void Execute()
    {
        if (change == 0) return;

        GlobalEvents.Rations.RationsChangeEvent(change);
    }
}
