using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    public class SelectableBase : Selectable,
        ISubmitHandler,
        IPointerClickHandler
    {
        public UnityEvent onSelect;
        public UnityEvent onSubmit;
        public UnityEvent onPointerEnter;

        public override void OnSelect(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            base.OnSelect(eventData);
            onSelect?.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            base.OnPointerEnter(eventData);
            onPointerEnter?.Invoke();

            Select();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            onSubmit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            onSubmit?.Invoke();
        }
    }
}
