using Game.Input;
using Game.UI;
using UnityEngine;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueDisplay : Singleton<DialogueDisplay>
    {
        public AnimatableTextDisplay text;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

        private Dialogue currentDialogue;

        protected override void HandleAwake()
        {
            animator = GetComponent<Animator>();
            animator.enabled = false;

            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
            isHidden = true;

            text.onTextComplete += OnTextComplete;
        }

        public void StartDialogue(Dialogue dialogue)
        {
            if (currentDialogue != null || dialogue == null) return;

            Show();

            currentDialogue = dialogue;
            text.SetText(dialogue.text);
            dialogue.onEnterState?.Invoke();

            InputManager.Instance.SubmitInput.OnPressEvent += OnSubmit;
        }

        private void TryNextDialogue(Dialogue dialogue)
        {
            if (dialogue == null)
            {
                ExitDialogue();
                return;
            }

            currentDialogue = dialogue;
            text.SetText(dialogue.text);
            dialogue.onEnterState?.Invoke();
        }

        private void ExitDialogue()
        {
            currentDialogue = null;
            Hide();
            InputManager.Instance.SubmitInput.OnPressEvent -= OnSubmit;
        }

        private void OnSubmit(IInput input)
        {
            if (text.IsAnimating)
            {
                text.SkipToEnd();
            }
            else if (currentDialogue.options.Count == 0)
            {
                TryNextDialogue(currentDialogue.defaultNextState);
            }
        }

        private void OnTextComplete()
        {
            if (currentDialogue == null) return;

            if (currentDialogue.options.Count > 0)
            {
                foreach (var option in currentDialogue.options)
                {
                    // create button (pooled) and bind event
                }
            }
            else
            {
                // display tap to continue
                if (currentDialogue.defaultNextState != null)
                {

                }
                else
                {
                }
            }
        }

        private void Show()
        {
            if (!isHidden) return;

            isHidden = false;
            animator.enabled = true;
            animator.Play(UIConstants.ShowAnimHash);
            HUDRoot.Instance.Hide();
        }

        private void Hide()
        {
            if (isHidden) return;

            isHidden = true;
            animator.enabled = true;
            animator.Play(UIConstants.HideAnimHash);
            HUDRoot.Instance.Show();
        }

        private void OnAnimationFinish()
        {
            animator.enabled = false;
            canvasGroup.interactable = !isHidden;
            canvasGroup.blocksRaycasts = !isHidden;
        }
    }
}
