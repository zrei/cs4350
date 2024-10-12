using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Game;
using UnityEngine;

public class SkillAnimationManager : MonoBehaviour
{
    [SerializeField] Transform m_AttackerPosition;
    [SerializeField] Transform m_TargetPosition;
    [SerializeField] CinemachineVirtualCamera m_SkillAnimVCam;

    private Vector3 m_CachedAttackerPosition;
    private Quaternion m_CachedAttackerRotation;
    private Vector3 m_CachedTargetPosition;
    private Quaternion m_CachedTargetRotation;

    private bool m_IsSelfTarget = false;
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
        m_IsSelfTarget = activeSkill.IsSelfTarget || (!activeSkill.IsAoe && targets[0].Equals(attacker));
        Logger.Log(this.GetType().Name, "Is self target: " + m_IsSelfTarget, LogLevel.LOG);

        StartCoroutine(PlayAttackAnimation(activeSkill, attacker, targets));
    }

    private IEnumerator PlayAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> targets)
    {
        void OnSkillHit()
        {
            attacker.AnimationEventHandler.onSkillHit -= OnSkillHit;

            TimeManager.Instance.ModifyTime(0.1f, 0.5f);
        }
        attacker.AnimationEventHandler.onSkillHit += OnSkillHit;

        var isSkillComplete = false;
        void OnSkillComplete()
        {
            attacker.AnimationEventHandler.onSkillComplete -= OnSkillComplete;

            isSkillComplete = true;
        }
        attacker.AnimationEventHandler.onSkillComplete += OnSkillComplete;

        Unit player = null;
        Unit enemy = null;
        // if it's a self target or an AOE skill, do not shift the units
        if (!m_IsSelfTarget && !activeSkill.IsAoe)
        {
            var target = targets[0];
            if (attacker.UnitAllegiance == UnitAllegiance.PLAYER)
            {
                player = attacker;
                enemy = target;
            }
            else
            {
                player = target;
                enemy = attacker;
            }

            m_SkillAnimVCam.enabled = true;

            var battleManager = BattleManager.Instance;
            foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(0, 0.25f);
            foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(0, 0.25f);
            yield return new WaitForSeconds(0.5f);

            m_CachedAttackerPosition = player.transform.position;
            m_CachedAttackerRotation = player.transform.rotation;
            player.transform.position = m_AttackerPosition.position;
            player.transform.rotation = m_AttackerPosition.rotation;
            m_CachedTargetPosition = enemy.transform.position;
            m_CachedTargetRotation = enemy.transform.rotation;
            enemy.transform.position = m_TargetPosition.position;
            enemy.transform.rotation = m_TargetPosition.rotation;
            player.FadeMesh(1, 0.25f);
            enemy.FadeMesh(1, 0.25f);
            yield return new WaitForSeconds(0.5f);
        }

        attacker.PlaySkillExecuteAnimation();

        if (activeSkill.m_TargetWillPlayHurtAnimation)
        {
            foreach (Unit target in targets)
                target.PlayAnimations(Unit.HurtAnimParam);
        }

        while (!isSkillComplete) yield return null;

        if (!m_IsSelfTarget && !activeSkill.IsAoe)
        {
            m_SkillAnimVCam.enabled = false;

            player.FadeMesh(0, 0.25f);
            enemy.FadeMesh(0, 0.25f);
            yield return new WaitForSeconds(0.5f);

            player.transform.position = m_CachedAttackerPosition;
            player.transform.rotation = m_CachedAttackerRotation;
            enemy.transform.position = m_CachedTargetPosition;
            enemy.transform.rotation = m_CachedTargetRotation;

            var battleManager = BattleManager.Instance;
            foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(1, 0.25f);
            foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(1, 0.25f);
            yield return new WaitForSeconds(0.5f);
        }

        GlobalEvents.Battle.CompleteAttackAnimationEvent?.Invoke();
    }
}