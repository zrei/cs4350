using UnityEngine;

public enum WeaponType
{
    SWORD,
    LANCE,
    AXE,
    BOW,
    MAGIC
}

[CreateAssetMenu(fileName = "WeaponSO", menuName = "ScriptableObject/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public WeaponType m_WeaponType;
    // public Mesh m_WeaponModel;
    public ActiveSkillSO[] m_WeaponActiveSkills;
}