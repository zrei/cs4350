using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField]
        private Image fill;

        [SerializeField]
        private FormattedTextDisplay text;

        private Coroutine animateCoroutine;

        public void SetValue(float targetValue, float maxValue, float transitionDuration = 0.5f)
        {
            if (maxValue == 0) return;

            if (animateCoroutine != null)
            {
                StopCoroutine(animateCoroutine);
                animateCoroutine = null;
            }

            if (transitionDuration <= 0)
            {
                fill.fillAmount = targetValue / maxValue;
                if (text != null) text.SetValue(targetValue, maxValue);
                return;
            }

            animateCoroutine = StartCoroutine(AnimateValueChange(targetValue, maxValue, transitionDuration));
        }

        private IEnumerator AnimateValueChange(float targetValue, float maxValue, float transitionDuration)
        {
            var currentFill = fill.fillAmount;
            var t = 0f;
            while (t < transitionDuration)
            {
                t += Time.unscaledDeltaTime;
                fill.fillAmount = Mathf.Lerp(currentFill, targetValue / maxValue, t / transitionDuration);
                if (text != null) text.SetValue(fill.fillAmount * maxValue, maxValue);
                yield return null;
            }

            fill.fillAmount = targetValue / maxValue;
            text.SetValue(targetValue, maxValue);
        }
    }
}
