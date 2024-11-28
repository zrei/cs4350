using UnityEngine;
using System.Collections.Generic;
using Game.UI;

[CreateAssetMenu(fileName="TutorialSO", menuName="ScriptableObject/Tutorial/TutorialSO")]
public class TutorialSO : ScriptableObject
{
    public List<TutorialPageUIData> m_TutorialPages;
}
