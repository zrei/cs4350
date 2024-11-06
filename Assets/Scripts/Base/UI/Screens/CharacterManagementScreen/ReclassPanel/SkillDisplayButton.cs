using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Game.UI
{
    public class SkillDisplayButton : ActionButton
    {
        public UnityEvent onPointerExit;

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            onPointerExit?.Invoke();
        }
    }
}
