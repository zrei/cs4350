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

    // tier 1 weapon model?
    public WeaponModel m_WeaponModel;
}
