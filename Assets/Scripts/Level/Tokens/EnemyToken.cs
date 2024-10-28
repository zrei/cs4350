namespace Level
{
    public class EnemyToken : LevelCharacterToken
    {
        public void Initialise(EnemyCharacterSO enemyCharacterSO)
        {
            var classSo = enemyCharacterSO.m_EnemyClass;
            var unitModelData = enemyCharacterSO.GetUnitModelData();
            var weaponInstanceSo = enemyCharacterSO.m_EquippedWeapon;
            
            Initialise(unitModelData, weaponInstanceSo, classSo);
        }
    }
}
