using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "PlayerCharacterIdHelper", menuName = "ScriptableObject/IdHelpers/PlayerCharacterIdHelper")]
public class PlayerCharacterIdHelper : IdHelper<PlayerCharacterSO>
{
    protected override string InstanceSoName => "PlayerCharacterSO";

    protected override void EditInstanceSoId(PlayerCharacterSO playerCharacterSO, int newId)
    {
        playerCharacterSO.m_Id = newId;
    }

    protected override int GetInstanceSoId(PlayerCharacterSO playerCharacterSO)
    {
        return playerCharacterSO.m_Id;
    }
}
#endif
