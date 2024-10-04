using UnityEngine;

public class CritTokenSO : TokenSO
{
    [Tooltip("Proportion to crit damage or heal amount - e.g. 0.2 will lead to a final value of 1.2 * initial value")]
    public float m_CritAmount;
}
