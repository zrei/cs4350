using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class AttackAnimationManager : MonoBehaviour
{
    [SerializeField] Transform m_AttackerPosition;
    [SerializeField] Transform m_TargetPosition;
    [SerializeField] Transform m_CameraPosition;

    private Vector3 m_CachedAttackerPosition;
    private Quaternion m_CachedAttackerRotation;
    private Vector3 m_CachedTargetPosition;
    private Quaternion m_CachedTargetRotation;

    private void Awake()
    {
        GlobalEvents.Battle.AttackAnimationEvent += OnSkillAnimation;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.AttackAnimationEvent -= OnSkillAnimation;
    }

    // perform attack skill
    private void OnSkillAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> targets)
    {
        // if it's a self target or an AOE skill, do not shift the units
        if (!activeSkill.m_LockToSelfTarget && !activeSkill.IsAoe)
        {
            Unit target = targets[0];

            Camera attackCamera = CameraManager.Instance.AttackAnimCamera;
            attackCamera.transform.position = m_CameraPosition.position;
            attackCamera.transform.rotation = m_CameraPosition.rotation;

            m_CachedAttackerPosition = attacker.transform.position;
            m_CachedAttackerRotation = attacker.transform.rotation;
            attacker.transform.position = m_AttackerPosition.position;
            attacker.transform.rotation = m_AttackerPosition.rotation;
            m_CachedTargetPosition = target.transform.position;
            m_CachedTargetRotation = target.transform.rotation;
            target.transform.position = m_TargetPosition.position;
            target.transform.rotation = m_TargetPosition.rotation;
            attackCamera.enabled = true;
        }

        StartCoroutine(PlayAttackAnimation(activeSkill, attacker, targets));
    }

    private IEnumerator PlayAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> targets)
    {
        attacker.PlayAttackAnimation(!activeSkill.DealsDamage);

        // for none damage dealing attacks and self attacks, there are no response animations, just VFX
        if (!activeSkill.m_LockToSelfTarget || !activeSkill.DealsDamage)
        {
            yield return new WaitForSeconds(activeSkill.m_DelayResponseAnimationTime);
            foreach (Unit target in targets)
                target.PlayAnimations(Unit.HurtAnimHash);
        }

        // need to account for hurt animation time and take the maximum of the end times
        yield return new WaitForSeconds(activeSkill.m_AnimationTime);

        if (!activeSkill.m_LockToSelfTarget && !activeSkill.IsAoe)
        {
            Unit target = targets[0];
            attacker.transform.position = m_CachedAttackerPosition;
            attacker.transform.rotation = m_CachedAttackerRotation;
            target.transform.position = m_CachedTargetPosition;
            target.transform.rotation = m_CachedTargetRotation;
        }

        CameraManager.Instance.AttackAnimCamera.enabled = false;
        GlobalEvents.Battle.CompleteAttackAnimationEvent?.Invoke();
    }
}