using UnityEngine;

[CreateAssetMenu(fileName = "ReflectTokenTierSO", menuName = "ScriptableObject/Inflictables/Token/ReflectTokenTierSO")]
public class ReflectTokenTierSO : TokenTierSO
{
    public override TokenType TokenType => TokenType.REFLECT;
    
    public float GetReflectProportion(int tier)
    {
        if (TryRetrieveTier(tier, out TokenSO tokenSO))
        {
            return tokenSO.m_Amount;
        }
        return 0f;
    }
}
