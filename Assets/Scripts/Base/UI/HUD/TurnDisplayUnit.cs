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

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            image = GetComponent<Image>();
            GlobalEvents.Battle.UnitDefeatedEvent += OnUnitDefeated;
        }

        private void OnUnitDefeated(Unit defeatedUnit)
        {
            if (defeatedUnit == unit)
            {
                GlobalEvents.Battle.UnitDefeatedEvent -= OnUnitDefeated;
                TurnDisplay.Instance.RemoveTurnDisplayUnit(unit);
                Destroy(gameObject);
            }
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
