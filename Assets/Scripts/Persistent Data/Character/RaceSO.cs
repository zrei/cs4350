using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

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
    public string m_MaleArmorFolder = "Assets/Models/Armors/";
    public string m_MaleMagePrefix = "M_Mage";
    public string m_MaleHoodedPrefix = "M_Rogue";
    public string m_MaleArmorPrefix = "M_Heavy";

    [Header("Female - helper")]
    public string m_FemaleArmorFolder = "Assets/Models/Armors/";
    public string m_FemaleMagePrefix = "F_Mage";
    public string m_FemaleHoodedPrefix = "F_Rogue";
    public string m_FemaleArmorPrefix = "F_Heavy";

    public void FillFemale()
    {
        FillFemaleArmorMeshes();
        FillFemaleHoodedMeshes();
        FillFemaleMageMeshes();
        EditorUtility.SetDirty(this);
    }

    public void FillMale()
    {
        FillMaleArmorMeshes();
        FillMaleHoodedMeshes();
        FillMaleMageMeshes();
        EditorUtility.SetDirty(this);
    }

    #region Armor
    private void FillFemaleMageMeshes()
    {
        m_FemaleItems.m_MageMeshes = GetSkinnedMeshRenderersInFolder(m_FemaleArmorFolder, m_FemaleMagePrefix);
    }

    private void FillFemaleHoodedMeshes()
    {
        m_FemaleItems.m_HoodedMeshes = GetSkinnedMeshRenderersInFolder(m_FemaleArmorFolder, m_FemaleHoodedPrefix);
    }

    private void FillFemaleArmorMeshes()
    {
        m_FemaleItems.m_ArmorMeshes = GetSkinnedMeshRenderersInFolder(m_FemaleArmorFolder, m_FemaleArmorPrefix);
    }

    private void FillMaleMageMeshes()
    {
        m_MaleItems.m_MageMeshes = GetSkinnedMeshRenderersInFolder(m_MaleArmorFolder, m_MaleMagePrefix);
    }

    private void FillMaleHoodedMeshes()
    {
        m_MaleItems.m_HoodedMeshes = GetSkinnedMeshRenderersInFolder(m_MaleArmorFolder, m_MaleHoodedPrefix);
    }

    private void FillMaleArmorMeshes()
    {
        m_MaleItems.m_ArmorMeshes = GetSkinnedMeshRenderersInFolder(m_MaleArmorFolder, m_MaleArmorPrefix);
    }
    #endregion

    private SkinnedMeshRenderer[] GetSkinnedMeshRenderersInFolder(string folderPath, string prefix)
    {
        List<SkinnedMeshRenderer> meshRenderers = new();
        foreach (string instancePath in AssetHelpers.FindAssetPathsByType("t: Model", true, folderPath))
        {
            if (!instancePath.Contains(prefix))
                continue;

            foreach (Object obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(instancePath))
            {
                if (!(obj is GameObject))
                    continue;

                SkinnedMeshRenderer meshRenderer = ((GameObject) obj).GetComponent<SkinnedMeshRenderer>();
            
                if (meshRenderer == null)
                    continue;

                meshRenderers.Add(meshRenderer);
            }
        }
        return meshRenderers.ToArray();
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

        GUILayout.Space(10);

        if (GUILayout.Button("Fill male"))
        {
            m_Target.FillMale();
        }

        if (GUILayout.Button("Fill female"))
        {
            m_Target.FillFemale();
        }
    }
}
#endif