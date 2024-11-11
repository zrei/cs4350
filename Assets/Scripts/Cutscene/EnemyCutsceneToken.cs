using UnityEngine;

public class EnemyCutsceneToken : BaseCharacterToken
{
    [SerializeField] EnemyCharacterSO m_EnemyCharacterSO;

    private void Start()
    {
        Initialise(m_EnemyCharacterSO.GetUnitModelData(), m_EnemyCharacterSO.m_EquippedWeapon, m_EnemyCharacterSO.m_EnemyClass);
    }
}
