using Game.Input;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        private NamedObjectButton dialogueButtonPrefab;

        #region Component References
        #region Character Sprite
        [SerializeField]
        private GameObject characterSpriteContainer;

        [SerializeField]
        private Image characterSprite;
        #endregion

        #region Character Name
        [SerializeField]
        private GameObject characterNameContainer;

        [SerializeField]
        private TextMeshProUGUI characterName;
        #endregion

        [SerializeField]
        private GraphicGroup graphicGroup;

        [SerializeField]
        private AnimatableTextDisplay text;

        [SerializeField]
        private LayoutGroup buttonsLayout;
        #endregion

        private Animator animator;
        private CanvasGroup canvasGroup;

        private bool isHidden;

        private Dialogue CurrentDialogue
        {
            set
            {
                currentDialogue = value;

                if (currentDialogue != null)
                {
                    text.SetText(currentDialogue.text);

                    graphicGroup.CrossFadeColor(currentDialogue.characterColor, 0.25f, false, false);

                    characterSprite.sprite = currentDialogue.characterSprite;
                    characterSpriteContainer.SetActive(currentDialogue.characterSprite != null);

                    characterName.text = currentDialogue.characterName;
                    characterNameContainer.SetActive(!string.IsNullOrEmpty(currentDialogue.characterName));
                }
            }
        }
        private Dialogue currentDialogue;

        private HashSet<NamedObjectButton> activeDisplays = new();

        private ObjectPool<NamedObjectButton> displayPool;

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
                actionOnGet: display => { activeDisplays.Add(display); display.gameObject.SetActive(true); display.transform.SetAsFirstSibling(); },
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

            CurrentDialogue = dialogue;
            dialogue.onEnterState?.Invoke();
            if (dialogue.audioCue != null)
            {
                SoundManager.Instance.Play(dialogue.audioCue);
            }

            InputManager.Instance.SubmitInput.OnPressEvent += OnSubmit;
            InputManager.Instance.PointerSelectInput.OnPressEvent += OnSubmit;
            
            GlobalEvents.Dialogue.DialogueStartEvent?.Invoke();
        }

        private void TryNextDialogue(Dialogue dialogue)
        {
            if (dialogue == null)
            {
                ExitDialogue();
                return;
            }

            CurrentDialogue = dialogue;
            dialogue.onEnterState?.Invoke();
            if (dialogue.audioCue != null)
            {
                SoundManager.Instance.Play(dialogue.audioCue);
            }
        }

        private void ExitDialogue()
        {
            CurrentDialogue = null;
            Hide();
            InputManager.Instance.SubmitInput.OnPressEvent -= OnSubmit;
            InputManager.Instance.PointerSelectInput.OnPressEvent -= OnSubmit;
            GlobalEvents.Dialogue.DialogueEndEvent?.Invoke();
        }

        private void OnSubmit(IInput input)
        {
            if (text.IsAnimating)
            {
                text.SkipToEnd();
            }
            else if (currentDialogue?.options?.Count == 0)
            {
                TryNextDialogue(currentDialogue.defaultNextState);
            }
        }

        private void OnTextComplete()
        {
            if (currentDialogue == null) return;

            if (currentDialogue.options.Count > 0)
            {
                //bool isFirst = true;
                foreach (var option in currentDialogue.options)
                {
                    var isUnlocked = option.IsUnlocked;
                    if (!isUnlocked && option.hideIfConditionsUnmet) continue;

                    var button = displayPool.Get();
                    button.nameText.text = isUnlocked ? option.text : $"{option.lockedText} {option.text}";
                    button.interactable = isUnlocked;

                    if (!isUnlocked) continue;
                    
                    var nextState = option.nextState;
                    button.onSubmit.RemoveAllListeners();
                    button.onSubmit.AddListener(() =>
                    {
                        if (option.changesMorality)
                        {
                            GlobalEvents.Morality.MoralityChangeEvent?.Invoke(option.moralityChange);
                        }

                        TryNextDialogue(nextState);
                        foreach (var active in activeDisplays.ToList())
                        {
                            displayPool.Release(active);
                        }
                    });

                    //if (isFirst)
                    //{
                    //    isFirst = false;
                    //    button.Select();
                    //}
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
