using UnityEngine;

#if UNITY_EDITOR
[CreateAssetMenu(fileName = "WeaponInstanceIdHelper", menuName = "ScriptableObject/IdHelpers/WeaponInstanceIdHelper")]
public class WeaponInstanceIdHelper : IdHelper<WeaponInstanceSO>
{
    protected override string InstanceSoName => "WeaponInstanceSO";

    protected override void EditInstanceSoId(WeaponInstanceSO weaponInstanceSO, int newId)
    {
        weaponInstanceSO.m_WeaponId = newId;
    }

    protected override int GetInstanceSoId(WeaponInstanceSO weaponInstanceSO)
    {
        return weaponInstanceSO.m_WeaponId;
    }
}
#endif
