using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "TokenTierIdHelper", menuName = "ScriptableObject/IdHelpers/TokenTierIdHelper")]
public class TokenTierIdHelper : IdHelper<TokenTierSO>
{
    protected override string InstanceSoName => "TokenTierSO";

    protected override void EditInstanceSoId(TokenTierSO tokenTierSO, int newId)
    {
        tokenTierSO.m_Id = newId;
    }

    protected override int GetInstanceSoId(TokenTierSO tokenTierSO)
    {
        return tokenTierSO.m_Id;
    }
}
#endif
