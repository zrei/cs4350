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

            IEnumerator ExecuteWithDelay()
            {
                yield return new WaitForSeconds(0.1f);
                TimeManager.Instance.ModifyTime(0.1f, 0.5f);
            }
            StartCoroutine(ExecuteWithDelay());

            activeSkill.m_SkillFXs.ForEach(x => x.Play(attacker, targets.Count > 0 ? targets[0] : null));
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
        var target = targets[0];
        if (attacker.UnitAllegiance == UnitAllegiance.PLAYER && target.UnitAllegiance == UnitAllegiance.ENEMY)
        {
            player = attacker;
            enemy = target;
        }
        else if (attacker.UnitAllegiance == UnitAllegiance.ENEMY && target.UnitAllegiance == UnitAllegiance.PLAYER)
        {
            player = target;
            enemy = attacker;
        }
        else if (m_IsSelfTarget)
        {
            player = target;
        }
        else if (attacker.UnitAllegiance == target.UnitAllegiance)
        {
            // todo: handle buff skill animation
            player = attacker;
            enemy = target;
        }

        var battleManager = BattleManager.Instance;

        m_SkillAnimVCam.enabled = true;

        foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(0, 0.25f);
        foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(0, 0.25f);
        yield return new WaitForSeconds(0.5f);

        if (player != null)
        {
            m_CachedAttackerPosition = player.transform.position;
            m_CachedAttackerRotation = player.transform.rotation;
            player.transform.position = m_AttackerPosition.position;
            player.transform.rotation = m_AttackerPosition.rotation;
            player.FadeMesh(1, 0.25f);
        }
        if (enemy != null)
        {
            m_CachedTargetPosition = enemy.transform.position;
            m_CachedTargetRotation = enemy.transform.rotation;
            enemy.transform.position = m_TargetPosition.position;
            enemy.transform.rotation = m_TargetPosition.rotation;
            enemy.FadeMesh(1, 0.25f);
        }
        yield return new WaitForSeconds(0.5f);

        attacker.PlaySkillExecuteAnimation();

        if (activeSkill.m_TargetWillPlayHurtAnimation)
        {
            foreach (Unit t in targets)
                t.PlayAnimations(Unit.HurtAnimParam);
        }

        while (!isSkillComplete) yield return null;

        m_SkillAnimVCam.enabled = false;

        player?.FadeMesh(0, 0.25f);
        enemy?.FadeMesh(0, 0.25f);
        yield return new WaitForSeconds(0.5f);

        if (player != null)
        {
            player.transform.position = m_CachedAttackerPosition;
            player.transform.rotation = m_CachedAttackerRotation;
        }
        if (enemy != null)
        {
            enemy.transform.position = m_CachedTargetPosition;
            enemy.transform.rotation = m_CachedTargetRotation;
        }

        foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(1, 0.25f);
        foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(1, 0.25f);
        yield return new WaitForSeconds(0.5f);

        GlobalEvents.Battle.CompleteAttackAnimationEvent?.Invoke();
    }
}