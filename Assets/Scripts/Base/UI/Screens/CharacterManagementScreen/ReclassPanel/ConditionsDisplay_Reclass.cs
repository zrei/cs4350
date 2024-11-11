using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class ConditionsDisplay_Reclass : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_ConditionText;

        public void SetDisplay(PathClass pathClass)
        {
            m_ConditionText.text = pathClass.m_UnlockCondition.GetDescription();
        }
    }
}
