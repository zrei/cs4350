using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponModelAttachmentType
{
    RIGHT_HAND,
    LEFT_HAND,
}

public class WeaponModel : MonoBehaviour
{
    private static readonly int SkillStartAnimParam = Animator.StringToHash("SkillStart");
    private static readonly int SkillExecuteAnimParam = Animator.StringToHash("SkillExecute");
    private static readonly int SkillCancelAnimParam = Animator.StringToHash("SkillCancel");
    private static readonly int SkillIDAnimParam = Animator.StringToHash("SkillID");

    public WeaponModelAttachmentType attachmentType;

    public List<Transform> fxAttachPoints = new();

    private Transform parent;

    private Animator m_Animator;
    private bool m_IsSkillAnimStarted;

    private void Awake()
    {
        m_Animator = GetComponentInChildren<Animator>();
    }

    public void PlaySkillStartAnimation(int skillID)
    {
        if (m_Animator == null) return;

        m_Animator.SetInteger(SkillIDAnimParam, skillID);
        m_Animator.ResetTrigger(SkillCancelAnimParam);
        m_Animator.ResetTrigger(SkillExecuteAnimParam);
        m_Animator.SetTrigger(SkillStartAnimParam);
        m_IsSkillAnimStarted = true;
    }

    public void PlaySkillExecuteAnimation()
    {
        if (m_Animator == null) return;

        if (!m_IsSkillAnimStarted) return;

        m_Animator.SetTrigger(SkillExecuteAnimParam);
        m_IsSkillAnimStarted = false;
        m_Animator.SetInteger(SkillIDAnimParam, 0);
    }

    public void CancelSkillAnimation()
    {
        if (m_Animator == null) return;

        if (!m_IsSkillAnimStarted) return;

        m_Animator.SetTrigger(SkillCancelAnimParam);
        m_IsSkillAnimStarted = false;
        m_Animator.SetInteger(SkillIDAnimParam, 0);
    }

    private void Unparent()
    {
        parent = transform.parent;
        transform.SetParent(null);
    }

    private void Reparent()
    {
        transform.SetParent(parent);
    }
}
