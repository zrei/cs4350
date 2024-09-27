using UnityEngine;

// account for outfit types somewhere along the way
[CreateAssetMenu(fileName = "ClassSO", menuName = "ScriptableObject/ClassSO")]
public class ClassSO : ScriptableObject
{
    [Header("Details")]
    public int m_Id;
    public string m_ClassName;
    public Sprite m_Icon;
    [Tooltip("Level at which this class is unlocked")]
    public int m_LevelLock;
    [Tooltip("Amount that character's base stats are augmented")]
    public Stats m_StatAugments;
    [Tooltip("Amount that character's growth rate is augmented")]
    public GrowthRate m_GrowthRateAugments;
    public WeaponSO m_Weapon;
    public PassiveSkillSO[] m_PassiveSkills;
    public TileType[] m_TraversableTileTypes = new TileType[] {TileType.NORMAL};
    public bool m_CanSwapTiles = false;
}