using UnityEngine;

public enum WeaponType
{
    SWORD,
    LANCE,
    AXE,
    BOW,
    MAGIC,
    SUPPORT
}

public class WeaponSO
{
    public WeaponType m_WeaponType;
    // public Mesh m_WeaponModel;
    public ActiveSkillSO[] m_WeaponActiveSkills;
}