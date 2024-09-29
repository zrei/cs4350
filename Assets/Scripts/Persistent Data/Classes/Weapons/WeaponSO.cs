using UnityEngine;

/*
public enum WeaponType
{
    SWORD,
    LANCE,
    AXE,
    BOW,
    MAGIC
}
*/

[CreateAssetMenu(fileName = "WeaponSO", menuName = "ScriptableObject/Classes/WeaponSO")]
public class WeaponSO : ScriptableObject
{
    public WeaponAnimationType m_WeaponAnimationType;
    public GameObject m_WeaponModel;
    public ActiveSkillSO[] m_WeaponActiveSkills;
    public string m_SupportAnimatorParam = "Support";
    public string m_AttackAnimatorParam = "Attack";
}
