using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "TileEffectIdHelper", menuName = "ScriptableObject/IdHelpers/TileEffectIdHelper")]
public class TileEffectIdHelper : IdHelper<TileEffectSO>
{
    protected override string InstanceSoName => "StatusEffectSO";

    protected override void EditInstanceSoId(TileEffectSO tileEffectSO, int newId)
    {
        tileEffectSO.m_Id = newId;
    }

    protected override int GetInstanceSoId(TileEffectSO tileEffectSO)
    {
        return tileEffectSO.m_Id;
    }
}
#endif
