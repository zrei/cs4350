using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class TurnDisplayUnit : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField]
        private RectTransform root;

        [SerializeField]
        private GraphicGroup graphicGroup;

        private RectTransform rectTransform;

        private Unit unit;

        private Coroutine animateCoroutine;

        public float TimeToAct { get; private set; }

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        private void OnDestroy()
        {
            GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDefeated;
        }

        private void OnUnitDefeated(Unit defeatedUnit)
        {
            if (defeatedUnit == unit)
            {
                if (animateCoroutine != null) StopCoroutine(animateCoroutine);
                GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDefeated;
                TurnDisplay.Instance.RemoveTurnDisplayUnit(unit);
            }
        }

        private void SetHighlightActive(bool active)
        {
            if (active)
            {
                if (animateCoroutine != null) StopCoroutine(animateCoroutine);
                animateCoroutine = StartCoroutine(Animate(Vector3.one * 2, Color.yellow));
            }
            else
            {
                var targetColor = unit.UnitAllegiance switch
                {
                    UnitAllegiance.PLAYER => Color.cyan,
                    UnitAllegiance.ENEMY => Color.red,
                    _ => Color.white
                };
                if (animateCoroutine != null) StopCoroutine(animateCoroutine);
                animateCoroutine = StartCoroutine(Animate(Vector3.one, targetColor));
            }
        }

        public void OnPreviewUnit(Unit unit)
        {
            SetHighlightActive(unit == this.unit);
        }

        private IEnumerator Animate(Vector3 targetScale, Color targetColor)
        {
            var scale = root.localScale;
            var color = graphicGroup.color;
            var t = 0f;
            var duration = 0.1f;
            var p = t / duration;
            while (t < duration)
            {
                root.localScale = Vector3.Lerp(scale, targetScale, p);
                graphicGroup.color = Color.Lerp(color, targetColor, p);
                t += Time.deltaTime;
                p = t / duration;
                yield return null;
            }
            root.localScale = targetScale;
            graphicGroup.color = targetColor;
            animateCoroutine = null;
        }

        public void Initialize(Unit unit)
        {
            this.unit = unit;
            switch (unit.UnitAllegiance)
            {
                case UnitAllegiance.PLAYER:
                    graphicGroup.color = Color.cyan;
                    break;
                case UnitAllegiance.ENEMY:
                    graphicGroup.color = Color.red;
                    break;
            }
            GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDefeated;
        }

        public void UpdateTurnValue(float timeToNext, float max)
        {
            TimeToAct = timeToNext;
            var ratio = timeToNext / max;
            var angle = ratio * 2 * Mathf.PI;
            rectTransform.anchoredPosition = TurnDisplay.Instance.Radius * new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle));
            var rot = rectTransform.localEulerAngles;
            rot.z = ratio * 360;
            rectTransform.localEulerAngles = rot;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            TurnDisplay.Instance.OnPreviewUnit(unit);
            unit.UnitMarker.SetColor(Color.yellow);
            unit.UnitMarker.SetMarkerType(UnitMarker.IconType.TimeToAct);
            unit.UnitMarker.SetActive(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TurnDisplay.Instance.OnPreviewUnit(null);
            unit.UnitMarker.SetActive(false);
        }
    }
}
