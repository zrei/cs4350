using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;

public class AttackAnimationManager : MonoBehaviour
{
    [SerializeField] Transform m_AttackerPosition;
    [SerializeField] Transform m_TargetPosition;

    private Vector3 m_CachedAttackerPosition;
    private Quaternion m_CachedAttackerRotation;
    private Vector3 m_CachedTargetPosition;
    private Quaternion m_CachedTargetRotation;

    private void Awake()
    {
        GlobalEvents.Battle.AttackAnimationEvent += OnAttackAnimation;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
    }

    // skill for dama
    private void OnAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> targets)
    {

        if (!activeSkill.IsAoe)
        {
            Unit target = targets[0];
            m_CachedAttackerPosition = attacker.transform.position;
            m_CachedAttackerRotation = attacker.transform.rotation;
            attacker.transform.position = m_AttackerPosition.position;
            attacker.transform.rotation = m_AttackerPosition.rotation;
            m_CachedTargetPosition = target.transform.position;
            m_CachedTargetRotation = target.transform.rotation;
            target.transform.position = m_TargetPosition.position;
            target.transform.rotation = m_TargetPosition.rotation;
            CameraManager.Instance.AttackAnimCamera.enabled = true;
        }

        StartCoroutine(PlayAttackAnimation(activeSkill, attacker, targets));
    }

    private IEnumerator PlayAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> targets)
    {
        attacker.PlayAttackAnimation(activeSkill.m_WeaponType);

        foreach (Unit target in targets)
            target.PlayAnimations(Unit.HurtAnimHash);

        // get some actual wait time
        yield return new WaitForSeconds(2f);

        if (!activeSkill.IsAoe)
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