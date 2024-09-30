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
    private static int AttackStartAnimParam = Animator.StringToHash("AttackStart");

    public WeaponModelAttachmentType attachmentType;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void PlayAttackAnimation()
    {
        animator?.SetTrigger(AttackStartAnimParam);
    }
}
