using UnityEngine;

[RequireComponent(typeof(ArmorVisual))]
[AddComponentMenu("CutsceneTriggerResponses/ChangeWeaponCutsceneTrigger")]
public class ChangeWeaponCutsceneTrigger : CutsceneTriggerResponse
{
    [Tooltip("Leave empty if you intend to remove the weapon")]
    [SerializeField] WeaponInstanceSO m_WeaponInstanceSO;
    [SerializeField] WeaponAnimationType m_WeaponAnimationType = WeaponAnimationType.SWORD;

    private ArmorVisual m_ArmorVisual;

    private void Start()
    {
        m_ArmorVisual = GetComponent<ArmorVisual>();
    }

    protected override void PerformTrigger()
    {
        m_ArmorVisual.ChangeWeapons(m_WeaponInstanceSO, m_WeaponAnimationType);
    }
}
