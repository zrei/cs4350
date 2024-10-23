using UnityEngine;

/// <summary>
/// Groups the information on a singular token together. This base class can be used for any token that
/// does not require additional parameters.
/// </summary>
[CreateAssetMenu(fileName = "TokenSO", menuName = "ScriptableObject/Inflictables/Token/TokenSO")]
public class TokenSO : ScriptableObject
{
    [Tooltip("Used for different purposes depending on the token")]
    public float m_Amount;
}
