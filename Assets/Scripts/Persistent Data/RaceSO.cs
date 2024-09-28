using UnityEngine;

[CreateAssetMenu(fileName = "RaceSO", menuName = "ScriptableObject/RaceSO")]
public class RaceSO : ScriptableObject
{
    [Header("Descriptions")]
    public string m_RaceName;

    [Header("Grid")]
    public float m_GridYOffset = 0f;

    [Header("Model - If both genders have the same model just use the same prefab")]
    public GameObject m_BaseMaleModel;
    public GameObject m_BaseFemaleModel;

    [Header("Outfits")]
    public SkinnedMeshRenderer[] m_MageMeshes;
    public SkinnedMeshRenderer[] m_HoodedMeshes;
    public SkinnedMeshRenderer[] m_ArmorMeshes;

    public SkinnedMeshRenderer[] GetAttachItems(OutfitType outfitType)
    {
        return outfitType switch
        {
            OutfitType.MAGE => m_MageMeshes,
            OutfitType.HOODED => m_HoodedMeshes,
            OutfitType.ARMOR => m_ArmorMeshes,
            _ => new SkinnedMeshRenderer[] {}
        };
    }
}
