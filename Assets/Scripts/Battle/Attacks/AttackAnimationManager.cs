using System.Collections;
using UnityEngine;

public class AttackAnimationManager : MonoBehaviour
{
    [SerializeField] Transform m_AttackerPosition;
    [SerializeField] Transform m_TargetPosition;

    private void Awake()
    {
        GlobalEvents.Battle.AttackAnimationEvent += OnAttackAnimation;
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.AttackAnimationEvent -= OnAttackAnimation;
    }

    // skill for dama
    private void OnAttackAnimation(WeaponType weaponType, Unit attacker, Unit target)
    {
        attacker.transform.position = m_AttackerPosition.position;
        target.transform.position = m_TargetPosition.position;

        StartCoroutine(PlayAttackAnimation(weaponType, attacker, target));
    }

    private IEnumerator PlayAttackAnimation(WeaponType weaponType, Unit attacker, Unit target)
    {
        attacker.PlayAttackAnimation(weaponType);
        target.PlayAnimations(Unit.HurtAnimHash);

        yield return new WaitForSeconds(2f);
        GlobalEvents.Battle.CompleteAttackAnimationEvent?.Invoke();
    }
}