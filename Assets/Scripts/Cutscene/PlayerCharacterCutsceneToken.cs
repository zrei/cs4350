using UnityEngine;

public class PlayerCharacterCutsceneToken : BaseCharacterToken
{
    [SerializeField] PlayerCharacterSO m_BaseData;

    private void Start()
    {
        int characterId = m_BaseData.m_Id;
        if (CharacterDataManager.Instance.TryRetrieveCharacterData(characterId, out PlayerCharacterData characterData))
            Initialise(m_BaseData.GetUnitModelData(characterData.CurrClass.m_OutfitType), characterData.GetWeaponInstanceSO(), characterData.CurrClass);
        else
            Initialise(m_BaseData.GetUnitModelData(m_BaseData.StartingClass.m_OutfitType), m_BaseData.StartingClass.DefaultWeapon, m_BaseData.StartingClass);
    }
}
