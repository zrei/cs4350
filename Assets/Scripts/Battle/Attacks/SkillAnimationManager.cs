using Cinemachine;
using System.Collections;
using System.Collections.Generic;
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
            // todo: change this to onSkillRelease, change current onSkillHit anim events to onSkillRelease
            attacker.AnimationEventHandler.onSkillHit -= OnSkillHit;

            // todo: move this to onSkillHit, add actual onSkillHit anim events
            IEnumerator ExecuteWithDelay()
            {
                yield return new WaitForSeconds(0.1f);
                TimeManager.Instance.ModifyTime(0.1f, 0.5f);
            }
            StartCoroutine(ExecuteWithDelay());

            if (activeSkill.m_TargetWillPlayHurtAnimation)
            {
                float volumeModifier = 1f / targets.Count;

                foreach (Unit t in targets)
                {
                    t.PlayAnimations(ArmorVisual.HurtAnimParam);
                    t.PlayHurtSound(volumeModifier);
                }
            }
            activeSkill.m_SkillFXs.ForEach(x => x.Play(attacker, targets));
        }
        attacker.AnimationEventHandler.onSkillHit += OnSkillHit;

        var isSkillComplete = false;
        void OnSkillComplete()
        {
            attacker.AnimationEventHandler.onSkillComplete -= OnSkillComplete;

            isSkillComplete = true;
        }
        attacker.AnimationEventHandler.onSkillComplete += OnSkillComplete;

        var battleManager = BattleManager.Instance;

        var isAttack = activeSkill.IsOpposingSideTarget;
        var isRanged = activeSkill.m_IsRangedAttack;

        // if buff skill, no cam anim
        // if attack skill
        // vcam always 3rd person follow caster, look at target
        // fade all units, then fade in caster and targets during cam transition
        // if ranged, don't move caster, rotate caster to face target avg position
        // if melee, move caster to just in front of most forward and center pos

        yield return HandleCamAnimTransitIn(attacker, targets, isAttack, isRanged);

        attacker.PlaySkillExecuteAnimation();

        while (!isSkillComplete) yield return null;

        yield return HandleCamAnimTransitOut(attacker, targets, isAttack, isRanged);

        GlobalEvents.Battle.CompleteAttackAnimationEvent?.Invoke();
    }

    IEnumerator HandleCamAnimTransitIn(Unit attacker, List<Unit> targets, bool isAttack, bool isRanged)
    {
        if (!isAttack) yield break;

        var follow = m_SkillAnimVCam.Follow;
        var lookAt = m_SkillAnimVCam.LookAt;

        if (isRanged)
        {
            follow.position = attacker.transform.position;
            var lookAtPos = Vector3.zero;
            targets.ForEach(x => lookAtPos += x.transform.position);
            lookAt.position = lookAtPos / targets.Count;

            follow.rotation = Quaternion.LookRotation(lookAt.position - follow.position);
        }
        else
        {
            var avgXY = Vector2.zero;
            var closestLocalZ = float.MaxValue;
            var closestZ = float.MaxValue;
            var lookAtPos = Vector3.zero;
            foreach (var target in targets)
            {
                if (Mathf.Abs(target.transform.localPosition.z) < Mathf.Abs(closestZ))
                {
                    closestLocalZ = target.transform.localPosition.z;
                    closestZ = target.transform.position.z;
                }
                avgXY += new Vector2(target.transform.position.x, target.transform.position.y);
                lookAtPos += target.transform.position;
            }

            closestZ += attacker.UnitAllegiance == UnitAllegiance.PLAYER
                ? -2f
                : 2f;
            avgXY /= targets.Count;
            follow.position = new Vector3(avgXY.x, avgXY.y, closestZ);
            lookAt.position = lookAtPos / targets.Count;

            follow.rotation = Quaternion.LookRotation(lookAt.position - follow.position);
        }

        if (attacker.UnitAllegiance == UnitAllegiance.PLAYER)
        {
            follow.localEulerAngles = Vector3.zero;
        }
        else
        {
            follow.localEulerAngles = new Vector3(0, 180, 0);
        }

        m_SkillAnimVCam.enabled = true;

        var battleManager = BattleManager.Instance;

        foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(0, 0.25f);
        foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(0, 0.25f);
        yield return new WaitForSeconds(0.5f);

        m_CachedAttackerPosition = attacker.transform.position;
        attacker.transform.position = follow.transform.position;
        attacker.FadeMesh(1, 0.25f);

        foreach (var target in targets)
        {
            target.FadeMesh(1, 0.25f);
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator HandleCamAnimTransitOut(Unit attacker, List<Unit> targets, bool isAttack, bool isRanged)
    {
        if (!isAttack) yield break;

        m_SkillAnimVCam.enabled = false;

        attacker.FadeMesh(0, 0.25f);
        targets.ForEach(x => x.FadeMesh(0, 0.25f));
        yield return new WaitForSeconds(0.5f);

        attacker.transform.position = m_CachedAttackerPosition;

        var battleManager = BattleManager.Instance;
        foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(1, 0.25f);
        foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(1, 0.25f);
        yield return new WaitForSeconds(0.5f);
    }
}