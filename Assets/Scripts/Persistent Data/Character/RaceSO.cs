using UnityEngine;

[CreateAssetMenu(fileName = "RaceSO", menuName = "ScriptableObject/Characters/RaceSO")]
public class RaceSO : ScriptableObject
{
    [Header("Descriptions")]
    public string m_RaceName;

    [Header("Gender differences")]
    public GenderItems m_MaleItems;
    public GenderItems m_FemaleItems;
   
    public UnitModelData GetUnitModelData(Gender gender, OutfitType outfitType)
    {
        return new UnitModelData(GetBaseModel(gender), GetAttachItems(gender, outfitType), GetYOffset(gender));
    }

    private GameObject GetBaseModel(Gender gender)
    {
        return gender == Gender.MALE ? m_MaleItems.m_BaseModel : m_FemaleItems.m_BaseModel;
    }

    private float GetYOffset(Gender gender)
    {
        return gender == Gender.MALE ? m_MaleItems.m_GridYOffset : m_FemaleItems.m_GridYOffset;
    }

    private SkinnedMeshRenderer[] GetAttachItems(Gender gender, OutfitType outfitType)
    {
        return gender switch
        {
            Gender.MALE => GetAttachItems(m_MaleItems, outfitType),
            Gender.FEMALE => GetAttachItems(m_FemaleItems, outfitType),
            _ => new SkinnedMeshRenderer[] {}
        };
    }

    private SkinnedMeshRenderer[] GetAttachItems(GenderItems genderItems, OutfitType outfitType)
    {
        return outfitType switch
        {
            OutfitType.MAGE => genderItems.m_MageMeshes,
            OutfitType.HOODED => genderItems.m_HoodedMeshes,
            OutfitType.ARMOR => genderItems.m_ArmorMeshes,
            _ => new SkinnedMeshRenderer[] {}
        };
    }
}

[System.Serializable]
public struct GenderItems
{
    [Header("Model")]
    public GameObject m_BaseModel;
    public float m_GridYOffset;

    [Header("Attach Items")]
    public SkinnedMeshRenderer[] m_MageMeshes;
    public SkinnedMeshRenderer[] m_HoodedMeshes;
    public SkinnedMeshRenderer[] m_ArmorMeshes;

}
