using UnityEngine;

public abstract class PassiveChangeTokenTierSO : TargetOtherUnitsTokenTierSO
{
    [Header("Stats to Regen")]
    public bool m_ChangeHealth = true;
    public bool m_ChangeMana = true;
}
