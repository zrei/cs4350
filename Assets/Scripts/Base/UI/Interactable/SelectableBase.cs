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
        [SerializeField]
        private bool isWorldSpace;

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

        public override void OnMove(AxisEventData eventData)
        {
            if (isWorldSpace)
            {
                var mainCamT = CameraManager.Instance.MainCamera.transform;
                Vector3 moveVector = eventData.moveDir switch
                {
                    MoveDirection.Left => -mainCamT.right,
                    MoveDirection.Right => mainCamT.right,
                    MoveDirection.Up => mainCamT.up,
                    MoveDirection.Down => -mainCamT.up,
                    _ => default,
                };
                var leftSimilarity = Vector3.Dot(-transform.right, moveVector);
                var rightSimilarity = Vector3.Dot(transform.right, moveVector);
                var upSimilarity = Vector3.Dot(transform.up, moveVector);
                var downSimilarity = Vector3.Dot(-transform.up, moveVector);
                var similarities = new float[] { leftSimilarity, rightSimilarity, upSimilarity, downSimilarity };
                var highest = Mathf.Max(similarities);
                if (highest == leftSimilarity) eventData.moveDir = MoveDirection.Left;
                else if (highest == rightSimilarity) eventData.moveDir = MoveDirection.Right;
                else if (highest == upSimilarity) eventData.moveDir = MoveDirection.Up;
                else if (highest == downSimilarity) eventData.moveDir = MoveDirection.Down;
            }
            base.OnMove(eventData);
        }
    }
}
