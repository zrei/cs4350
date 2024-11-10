using System.Collections.Generic;
using Game.UI;
using UnityEngine;

/// <summary>
/// Text display with multiple texts (E.g. for two text with different alignment)
/// </summary>
public class MultiTextDisplay : MonoBehaviour
{
    [SerializeField] private FormattedTextDisplay m_PrimaryText;
    public FormattedTextDisplay PrimaryText => m_PrimaryText;
    
    [SerializeField] private FormattedTextDisplay m_SecondaryText;
    public FormattedTextDisplay SecondaryText => m_SecondaryText;
}