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

        [SerializeField]
        private bool deselectOnPointerExit;

        [SerializeField]
        [Tooltip("Leave empty for no sound")]
        private AudioDataSO hoverAudio;

        [SerializeField]
        [Tooltip("Leave empty for no sound")]
        private AudioDataSO selectAudio;

        [SerializeField]
        [Tooltip("Leave empty for no sound")]
        private AudioDataSO submitAudio;

        public UnityEvent onSelect;
        public UnityEvent onDeselect;
        public UnityEvent onSubmit;
        public UnityEvent onPointerEnter;
        public UnityEvent onPointerExit;

        public override void OnSelect(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            base.OnSelect(eventData);
            if (selectAudio != null)
                SoundManager.Instance.Play(selectAudio);
            onSelect?.Invoke();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            onDeselect?.Invoke();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            base.OnPointerEnter(eventData);
            if (hoverAudio != null)
                SoundManager.Instance.Play(hoverAudio);
            onPointerEnter?.Invoke();

            Select();
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            onPointerExit?.Invoke();

            if (deselectOnPointerExit)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }

        public void OnSubmit(BaseEventData eventData)
        {
            if (!IsInteractable()) return;

            if (submitAudio != null)
                SoundManager.Instance.Play(submitAudio);

            onSubmit?.Invoke();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable()) return;

            if (submitAudio != null)
                SoundManager.Instance.Play(submitAudio);

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
