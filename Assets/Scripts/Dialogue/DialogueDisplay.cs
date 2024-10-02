using Game.Input;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogueDisplay : Singleton<DialogueDisplay>
    {
        private const int MaxOptions = 5;

        [SerializeField]
        private DialogueButton dialogueButtonPrefab;

        [SerializeField]
        private AnimatableTextDisplay text;

        [SerializeField]
        private LayoutGroup buttonsLayout;

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

        private Dialogue currentDialogue;

        private HashSet<DialogueButton> activeDisplays = new();

        private ObjectPool<DialogueButton> displayPool;

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

            displayPool = new(
                createFunc: () => Instantiate(dialogueButtonPrefab, buttonsLayout.transform),
                actionOnGet: display => { activeDisplays.Add(display); display.gameObject.SetActive(true); },
                actionOnRelease: display => { activeDisplays.Remove(display); display.gameObject.SetActive(false); },
                actionOnDestroy: display => Destroy(display.gameObject),
                collectionCheck: true,
                defaultCapacity: 3,
                maxSize: MaxOptions
            );
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
                    var button = displayPool.Get();
                    button.text.text = option.text;
                    var nextState = option.nextState;
                    button.onSubmit.RemoveAllListeners();
                    button.onSubmit.AddListener(() =>
                    {
                        TryNextDialogue(nextState);
                        foreach (var active in activeDisplays.ToList())
                        {
                            displayPool.Release(active);
                        }
                    });
                }
            }
            else
            {
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
