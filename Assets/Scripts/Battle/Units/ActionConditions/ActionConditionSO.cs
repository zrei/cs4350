using UnityEngine;

/// <summary>
/// Generic enough to be used in multiple contexts
/// </summary>
public abstract class ActionConditionSO : ScriptableObject
{
    public bool m_IsInverted;

    public abstract bool IsConditionMet(Unit unit, MapLogic mapLogic);
}
