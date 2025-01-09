using UnityEngine;

public abstract class PassiveChangeTokenTierSO : TargetOtherTilesTokenTierSO
{
    [Header("Stats to Regen")]
    public bool m_ChangeHealth = true;
    public bool m_ChangeMana = true;
}
