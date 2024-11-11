using Level;
using UnityEngine;

[CreateAssetMenu(fileName = "FlagTriggerActionSO", menuName = "ScriptableObject/DataAction/FlagTriggerActionSO")]
public class FlagTriggerActionSO : DataActionSO
{
    public FlagTrigger[] flagResults;

    public override void Execute()
    {
        foreach (var flag in flagResults)
        {
            FlagManager.Instance.SetFlagValue(flag.flagName, flag.flagValue, flag.flagType);
        }
    }
}
