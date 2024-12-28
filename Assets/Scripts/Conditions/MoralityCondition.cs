using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "MoralityCondition", menuName = "ScriptableObject/Conditions/MoralityCondition")]
public class MoralityCondition : Condition
{
    public enum Mode
    {
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
    }

    public Mode mode;
    public int threshold;

    public override bool Evaluate()
    {
        if (!MoralityManager.IsReady) return false;
        
        var morality = MoralityManager.Instance.CurrMorality;
        switch (mode)
        {
            case Mode.GreaterThan:
                return morality > threshold;
            case Mode.GreaterThanOrEqual:
                return morality >= threshold;
            case Mode.LessThan:
                return morality < threshold;
            case Mode.LessThanOrEqual:
                return morality <= threshold;
        }
        return false;
    }
}
