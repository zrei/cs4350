namespace Level
{
    public class PlayerToken : LevelCharacterToken
    {
        public void Initialise(PlayerCharacterBattleData characterBattleData)
        {
            var classSo = characterBattleData.m_ClassSO;
            var unitModelData = characterBattleData.GetUnitModelData();
            var weaponInstanceSo = characterBattleData.m_CurrEquippedWeapon;
            
            Initialise(unitModelData, weaponInstanceSo, classSo);
        }        

        public void UpdateAppearance(PlayerCharacterBattleData characterBattleData)
        {
            var classSo = characterBattleData.m_ClassSO;
            var unitModelData = characterBattleData.GetUnitModelData();
            var weaponInstanceSo = characterBattleData.m_CurrEquippedWeapon;
            
            ChangeAppearance(unitModelData, weaponInstanceSo, classSo);
        }
    }
}
