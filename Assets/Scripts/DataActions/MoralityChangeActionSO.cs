using UnityEngine;

[CreateAssetMenu(fileName = "MoralityChangeActionSO", menuName = "ScriptableObject/DataAction/MoralityChangeActionSO")]
public class MoralityChangeActionSO : DataActionSO
{
    public int change;

    public override void Execute()
    {
        if (change == 0) return;

        GlobalEvents.Morality.MoralityChangeEvent(change);
    }
}
