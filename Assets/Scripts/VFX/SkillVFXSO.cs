using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillVFXSO", menuName = "ScriptableObject/VFX/SkillVFXSO")]
public class SkillVFXSO : VFXSO
{
    public enum PrimaryAttachmentType
    {
        WeaponModel,
        Caster,
    }

    public PrimaryAttachmentType m_AttachmentType;

    public int m_WeaponModelIndex;
    public int m_AttachPointIndex;

    public void Play(Unit caster, List<Unit> targets = null, Color? color = null, float? duration = null, bool unscaledTime = false)
    {
        Transform primaryAttachPoint = null;
        if (m_AttachmentType == PrimaryAttachmentType.WeaponModel &&
            m_Type != Type.StationaryAttachSecondaryFirst &&
            m_Type != Type.StationaryAttachSecondaryAll)
        {
            var weaponModels = caster.WeaponModels;
            var weaponModelIndex = Mathf.Clamp(m_WeaponModelIndex, 0, weaponModels.Count);
            if (weaponModelIndex < 0 || weaponModelIndex >= weaponModels.Count) return;
            var weaponModel = weaponModels[weaponModelIndex];

            var attachPoints = weaponModel.fxAttachPoints;
            var attachPointIndex = Mathf.Clamp(m_AttachPointIndex, 0, attachPoints.Count);
            if (attachPointIndex < 0 || attachPointIndex >= attachPoints.Count) return;
            primaryAttachPoint = attachPoints[attachPointIndex];
        }
        else
        {
            primaryAttachPoint = caster.transform;
        }

        if (primaryAttachPoint == null)
        {
            Debug.LogWarning($"{name} skill vfx failed to play: null primary attach point");
            return;
        }

        var shouldUseBodyCenter = m_Type == Type.MovingPrimaryToSecondaryFirst ||
            m_Type == Type.MovingPrimaryToSecondaryAll ||
            m_Type == Type.MovingSequential;

        Play(primaryAttachPoint, targets.ConvertAll(x => shouldUseBodyCenter ? x.BodyCenter : x.transform), color, duration, unscaledTime);
    }
}
