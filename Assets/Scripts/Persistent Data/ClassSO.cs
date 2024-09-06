using UnityEngine;

// account for outfit types somewhere along the way
public class ClassSO : ScriptableObject
{
    [Header("Details")]
    public string m_Name;
    public Sprite m_Icon;
    [Tooltip("Level at which this class is unlocked")]
    public int m_LevelLock;
    [Tooltip("Amount that character's base stats are augmented")]
    public Stats m_StatAugments;
    [Tooltip("Amount that character's growth rate is augmented")]
    public Stats m_GrowthRateAugments;
    public WeaponSO[] m_Weapons;
    public PassiveSkillSO[] m_PassiveSkills;
}