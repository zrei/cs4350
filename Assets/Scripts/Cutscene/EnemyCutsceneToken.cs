using UnityEngine;

public class EnemyCutsceneToken : CutsceneToken
{
    [SerializeField] EnemyCharacterSO m_EnemyCharacterSO;

    protected override void Initialise()
    {
        base.Initialise();

        if (m_EnemyCharacterSO == null)
        {
            Logger.Log(this.GetType().Name, this.name, "Enemy character SO not set!", this.gameObject, LogLevel.ERROR);
            return;
        }

        Initialise(m_EnemyCharacterSO.GetUnitModelData(), m_SpawnWeapon ? m_EnemyCharacterSO.m_EquippedWeapon : null, m_EnemyCharacterSO.m_EnemyClass);
    }
}
