using UnityEngine;

[CreateAssetMenu(fileName = "PlayerClassIdHelper", menuName = "ScriptableObject/IdHelpers/PlayerClassIdHelper")]
public class PlayerClassIdHelper : IdHelper<PlayerClassSO>
{
    protected override string InstanceSoName => "PlayerClassSO";

    protected override void EditInstanceSoId(PlayerClassSO playerClassSO, int newId)
    {
        playerClassSO.m_Id = newId;
    }

    protected override int GetInstanceSoId(PlayerClassSO playerClassSO)
    {
        return playerClassSO.m_Id;
    }
}
