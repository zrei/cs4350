using UnityEngine;

/// <summary>
/// Cutscene token where you directly specify the race, outfit and armor color
/// </summary>
public class NPCCutsceneToken : CutsceneToken
{
    [Header("Model")]
    [SerializeField] private RaceSO m_RaceSO;
    [SerializeField] private Gender m_Gender;
    
    [Header("Armor")]
    [SerializeField] private OutfitType m_OutfitType;
    [SerializeField] private Color m_ArmorPlateColor;
    [SerializeField] private Color m_ArmorTrimColor;
    [SerializeField] private Color m_UnderArmorColor;

    [Header("Weapon")]
    [SerializeField] private WeaponInstanceSO m_WeaponInstance;
    [SerializeField] private WeaponAnimationType m_WeaponAnimationType;

    protected override void Initialise()
    {
        base.Initialise();

        m_ArmorVisual.InstantiateModel(m_RaceSO.GetUnitModelData(m_Gender, m_OutfitType), m_ArmorPlateColor, m_ArmorTrimColor, m_UnderArmorColor, m_SpawnWeapon ? m_WeaponInstance : null, m_WeaponAnimationType);
    }
}
