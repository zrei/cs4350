using UnityEngine;

/// <summary>
/// Groups the information on a singular token together. This base class can be used for any token that
/// does not require additional parameters.
/// </summary>
[CreateAssetMenu(fileName = "TokenSO", menuName = "ScriptableObject/Inflictables/Token/TokenSO")]
public class TokenSO : ScriptableObject
{
    [Header("Details")]
    public string m_TokenName;
    [TextArea]
    public string m_Description;
    public Sprite m_Icon;
    [Tooltip("Used for different purposes depending on the token")]
    public float m_Amount;
    public string m_DisplayAmountFormat = "{0:G5}";
    public Color m_Color = Color.white;
    // public virtual TokenType TokenType => TokenType.INFLICT_STATUS;
}
