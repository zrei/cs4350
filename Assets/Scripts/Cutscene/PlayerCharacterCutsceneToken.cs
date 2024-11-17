using UnityEngine;

public class PlayerCharacterCutsceneToken : CutsceneToken
{
    [SerializeField] PlayerCharacterSO m_BaseData;

    protected override void Initialise()
    {
        base.Initialise();

        if (m_BaseData == null)
        {
            Logger.Log(this.GetType().Name, this.name, "Player character SO not set!", this.gameObject, LogLevel.ERROR);
            return;
        }

        int characterId = m_BaseData.m_Id;
        if (CharacterDataManager.IsReady && CharacterDataManager.Instance.TryRetrieveCharacterData(characterId, out PlayerCharacterData characterData))
            Initialise(m_BaseData.GetUnitModelData(characterData.CurrClass.m_OutfitType), m_SpawnWeapon ? characterData.GetWeaponInstanceSO() : null, characterData.CurrClass);
        else
            Initialise(m_BaseData.GetUnitModelData(m_BaseData.StartingClass.m_OutfitType), m_SpawnWeapon ? m_BaseData.StartingClass.DefaultWeapon : null, m_BaseData.StartingClass);
    }
}
