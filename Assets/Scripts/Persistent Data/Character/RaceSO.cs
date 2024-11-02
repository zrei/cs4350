using UnityEngine;
using UnityEditor;

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

#if UNITY_EDITOR
    [Header("Male - Helper")]
    public string m_FemaleMageFolder;
    public string m_FemaleHoodedFolder;
    public string m_FemaleArmorFolder;

    [Header("Female - helper")]
    public string m_MaleMageFolder;
    public string m_MaleHoodedFolder;
    public string m_MaleArmorFolder;

    public void FillFemaleMageMeshes()
    {

    }

    public void FillFemaleHoodedMeshes()
    {

    }

    public void FillFemaleArmorMeshes()
    {

    }

    public void FillMaleMageMeshes()
    {

    }

    public void FillMaleHoodedMeshes()
    {

    }

    public void FillMaleArmorMeshes()
    {

    }

    private SkinnedMeshRenderer[] GetSkinnedMeshRenderersInFolder(string folderPath)
    {
        foreach (string path in Asset)
    }
#endif
}

[System.Serializable]
public class GenderItems
{
    [Header("Model")]
    public GameObject m_BaseModel;
    public float m_GridYOffset;

    [Header("Attach Items")]
    public SkinnedMeshRenderer[] m_MageMeshes;
    public SkinnedMeshRenderer[] m_HoodedMeshes;
    public SkinnedMeshRenderer[] m_ArmorMeshes;
}

#if UNITY_EDITOR
[CustomEditor(typeof(RaceSO))]
public class RaceSOEditor : Editor
{
    private RaceSO m_Target;

    private void OnEnable()
    {
        m_Target = (RaceSO) target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif