using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            base.OnSelect(eventData);
            onSelect?.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            onPointerEnter?.Invoke();

            Select();
        }

        public void OnSubmit(BaseEventData eventData)
        {
            onSubmit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onSubmit?.Invoke();
        }
    }
}
