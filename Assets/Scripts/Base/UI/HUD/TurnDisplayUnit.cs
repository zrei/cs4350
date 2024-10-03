using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class TurnDisplayUnit : MonoBehaviour
    {
        private Image image;
        private RectTransform rectTransform;

        private Unit unit;

        private Coroutine animateCoroutine;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDefeated;
            GlobalEvents.Battle.PreviewUnitEvent += OnPreviewUnit;
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDefeated;
            GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
        }

        private void OnUnitDefeated(Unit defeatedUnit)
        {
            if (defeatedUnit == unit)
            {
                if (animateCoroutine != null) StopCoroutine(animateCoroutine);
                GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDefeated;
                GlobalEvents.Battle.PreviewUnitEvent -= OnPreviewUnit;
                TurnDisplay.Instance.RemoveTurnDisplayUnit(unit);
                Destroy(gameObject);
            }
        }

        private void OnPreviewUnit(Unit unit)
        {
            if (unit == this.unit)
            {
                if (animateCoroutine != null) StopCoroutine(animateCoroutine);
                animateCoroutine = StartCoroutine(Animate(Vector3.one * 2, Color.yellow));
            }
            else
            {
                var targetColor = this.unit.UnitAllegiance switch
                {
                    UnitAllegiance.PLAYER => Color.cyan,
                    UnitAllegiance.ENEMY => Color.red,
                    _ => Color.white
                };
                if (animateCoroutine != null) StopCoroutine(animateCoroutine);
                animateCoroutine = StartCoroutine(Animate(Vector3.one, targetColor));
            }
        }

        private IEnumerator Animate(Vector3 targetScale, Color targetColor)
        {
            var scale = transform.localScale;
            var color = image.color;
            var t = 0f;
            var duration = 0.1f;
            var p = t / duration;
            while (t < duration)
            {
                transform.localScale = Vector3.Lerp(scale, targetScale, p);
                image.color = Color.Lerp(color, targetColor, p);
                t += Time.deltaTime;
                p = t / duration;
                yield return null;
            }
            transform.localScale = targetScale;
            image.color = targetColor;
            animateCoroutine = null;
        }

        public void Initialize(Unit unit)
        {
            this.unit = unit;
            switch (unit.UnitAllegiance)
            {
                case UnitAllegiance.PLAYER:
                    image.color = Color.cyan;
                    break;
                case UnitAllegiance.ENEMY:
                    image.color = Color.red;
                    break;
            }
        }

        public void UpdateTurnValue(float timeToNext, float max)
        {
            var ratio = timeToNext / max;
            var angle = ratio * 2 * Mathf.PI;
            rectTransform.anchoredPosition = TurnDisplay.Instance.radius * new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
            var rot = rectTransform.localEulerAngles;
            rot.z = ratio * 360;
            rectTransform.localEulerAngles = rot;
        }
    }
}
