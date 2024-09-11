using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FormattedTextDisplay : MonoBehaviour
    {
        private TextMeshProUGUI Text
        {
            get => text == null ? text = GetComponent<TextMeshProUGUI>() : text;
        }
        private TextMeshProUGUI text;

        [TextArea]
        public string textFormat;

        public void SetValue(params object[] args)
        {
            Text.text = string.Format(textFormat, args);
        }
    }
}
