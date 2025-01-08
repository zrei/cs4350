using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillAnimationManager : Singleton<SkillAnimationManager>
{
    private const float MeleePositionOffset = 2f;

    [SerializeField] CinemachineVirtualCamera m_SkillAnimVCam;

    private Vector3 m_CachedAttackerPosition;
    private Quaternion m_CachedAttackerRotation;

    private bool m_ResetAttackerPosition;
    private bool m_ResetAttackerRotation;

    protected override void HandleAwake()
    {
        base.HandleAwake();
    }

    protected override void HandleDestroy()
    {
        base.HandleDestroy();
    }

    public void OnSkillAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> targets, Vector3? targetMovePosition)
    {
        StartCoroutine(PlayAttackAnimation(activeSkill, attacker, targets, targetMovePosition));
    }

    private IEnumerator PlayAttackAnimation(ActiveSkillSO activeSkill, Unit attacker, List<Unit> targets, Vector3? targetMovePosition)
    {
        GlobalEvents.Battle.AttackAnimationEvent?.Invoke();

        var onReleaseEndStopActions = new List<VoidEvent>(); // invoke stop on certain vfx on release end
        var waitForVFXInvokeHit = activeSkill.m_OnReleaseSkillVFXs.Any(x => x.m_InvokeHitEvent); // allow vfx to drive timing
        var isSkillHitInvoked = false; // ensure OnSkillHit is invoked exactly once
        var isSkillComplete = false;

        bool canExtendTurn = false;

        void OnSkillRelease()
        {
            attacker.AnimationEventHandler.onSkillRelease -= OnSkillRelease;

            foreach (var skillVFX in activeSkill.m_OnReleaseSkillVFXs)
            {
                var stopAction = skillVFX.Play(attacker, targets, onComplete: skillVFX.m_InvokeHitEvent ? OnSkillHit : null);
                if (skillVFX.m_StopOnReleaseEnd)
                {
                    onReleaseEndStopActions.Add(stopAction);
                }
            }
        }

        void OnSkillReleaseEnd()
        {
            attacker.AnimationEventHandler.onSkillReleaseEnd -= OnSkillReleaseEnd;

            onReleaseEndStopActions.ForEach(x => x?.Invoke());
        }

        void OnSkillHit()
        {
            if (isSkillHitInvoked) return;
            isSkillHitInvoked = true;

            attacker.AnimationEventHandler.onSkillHit -= OnSkillHit;

            attacker.ApplySkillEffects(activeSkill, targets, out canExtendTurn);

            if (activeSkill.m_TargetWillPlayHurtAnimation)
            {
                float volumeModifier = 1f / targets.Count;

                foreach (Unit t in targets)
                {
                    t.PlayAnimations(ArmorVisual.HurtAnimParam);
                    t.PlayHurtSound(volumeModifier);
                }
            }

            activeSkill.m_OnHitSkillVFXs.ForEach(x => x.Play(attacker, targets));

            CoroutineManager.Instance.ExecuteAfterDelay(() => TimeManager.Instance.ModifyTime(0.1f, 0.5f), 0.1f);
        }

        void OnSkillComplete()
        {
            isSkillComplete = true;

            attacker.AnimationEventHandler.onSkillComplete -= OnSkillComplete;
        }

        attacker.AnimationEventHandler.onSkillRelease += OnSkillRelease;
        attacker.AnimationEventHandler.onSkillReleaseEnd += OnSkillReleaseEnd;
        if (!waitForVFXInvokeHit)
        {
            attacker.AnimationEventHandler.onSkillHit += OnSkillHit;
        }
        attacker.AnimationEventHandler.onSkillComplete += OnSkillComplete;

        var isAttack = activeSkill.IsOpposingSideTarget;
        var isRanged = activeSkill.m_IsRangedAttack;

        // vcam always 3rd person follow caster, look at target
        // fade all units, then fade in caster and targets during cam transition
        // treat support skills i.e. not IsOpposingSideTarget like ranged
        // if ranged, don't move caster
        // if ranged attack, rotate caster to face target avg position
        // if melee, move caster to just in front of most forward and center pos

        yield return HandleCamAnimTransitIn(attacker, targets, isAttack, isRanged);

        attacker.PlaySkillExecuteAnimation();

        while (!isSkillHitInvoked || !isSkillComplete) yield return null;

        yield return HandleCamAnimTransitOut(attacker, targets, isAttack, isRanged);

        if (targetMovePosition.HasValue)
        {
            yield return HandleMoveSkillAnimation(activeSkill.m_TeleportSelf ? attacker : targets[0], targetMovePosition.Value);
        }

        GlobalEvents.Battle.CompleteAttackAnimationEvent?.Invoke(canExtendTurn);
    }

    IEnumerator HandleCamAnimTransitIn(Unit attacker, List<Unit> targets, bool isAttack, bool isRanged)
    {
        var follow = m_SkillAnimVCam.Follow;
        var lookAt = m_SkillAnimVCam.LookAt;

        if (isRanged || !isAttack)
        {
            follow.position = attacker.transform.position;
            var lookAtPos = Vector3.zero;
            if (targets.Count == 1 && attacker == targets[0])
            {
                lookAtPos = attacker.BodyCenter.position + attacker.transform.forward * MeleePositionOffset;
            }
            else
            {
                targets.ForEach(x => lookAtPos += x.BodyCenter.position);
                lookAtPos /= targets.Count;
            }
            lookAt.position = lookAtPos;

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
                if (Mathf.Abs(target.transform.localPosition.z) < Mathf.Abs(closestLocalZ))
                {
                    closestLocalZ = target.transform.localPosition.z;
                    closestZ = target.transform.position.z;
                }
                avgXY += new Vector2(target.transform.position.x, target.transform.position.y);
                lookAtPos += target.BodyCenter.position;
            }

            avgXY /= targets.Count;
            follow.position = new Vector3(avgXY.x, avgXY.y, closestZ);
            follow.position += attacker.transform.forward * MeleePositionOffset;
            lookAt.position = lookAtPos / targets.Count;

            follow.rotation = Quaternion.LookRotation(lookAt.position - follow.position);
        }

        m_SkillAnimVCam.enabled = true;

        var battleManager = BattleManager.Instance;

        foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(0, 0.25f);
        foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(0, 0.25f);
        yield return new WaitForSeconds(0.5f);

        m_CachedAttackerPosition = attacker.transform.position;
        attacker.transform.position = follow.transform.position;
        m_ResetAttackerPosition = true;

        if (isAttack)
        {
            m_CachedAttackerRotation = attacker.transform.rotation;
            var attackerRot = attacker.transform.eulerAngles;
            attackerRot.y = follow.transform.eulerAngles.y + 180;
            attacker.transform.eulerAngles = attackerRot;
            m_ResetAttackerRotation = true;
        }

        attacker.FadeMesh(1, 0.25f);

        foreach (var target in targets)
        {
            target.FadeMesh(1, 0.25f);
        }
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator HandleCamAnimTransitOut(Unit attacker, List<Unit> targets, bool isAttack, bool isRanged)
    {
        m_SkillAnimVCam.enabled = false;

        attacker.FadeMesh(0, 0.25f);
        targets.ForEach(x => x.FadeMesh(0, 0.25f));
        yield return new WaitForSeconds(0.5f);

        if (m_ResetAttackerPosition)
        {
            attacker.transform.position = m_CachedAttackerPosition;
            m_ResetAttackerPosition = false;
        }
        if (m_ResetAttackerRotation)
        {
            attacker.transform.rotation = m_CachedAttackerRotation;
            m_ResetAttackerRotation = false;
        }

        var battleManager = BattleManager.Instance;
        foreach (var unit in battleManager.PlayerUnits) unit.FadeMesh(1, 0.25f);
        foreach (var unit in battleManager.EnemyUnits) unit.FadeMesh(1, 0.25f);
        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator HandleMoveSkillAnimation(Unit target, Vector3 targetMovePosition)
    {
        var t = 0f;
        var duration = 0.1f;
        var progress = t / duration;
        var startPos = target.transform.position;

        while (t < duration)
        {
            t += Time.deltaTime;
            progress = t / duration;

            target.transform.position = Vector3.Lerp(startPos, targetMovePosition, progress);
            yield return null;
        }

        target.transform.position = targetMovePosition;
    }
}