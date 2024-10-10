using UnityEngine;

[CreateAssetMenu(fileName = "StatusEffectIdHelper", menuName = "ScriptableObject/IdHelpers/StatusEffectIdHelper")]
public class StatusEffectIdHelper : IdHelper<StatusEffectSO>
{
    protected override string InstanceSoName => "StatusEffectSO";

    protected override void EditInstanceSoId(StatusEffectSO statusEffectSO, int newId)
    {
        statusEffectSO.m_Id = newId;
    }

    protected override int GetInstanceSoId(StatusEffectSO statusEffectSO)
    {
        return statusEffectSO.m_Id;
    }
}
