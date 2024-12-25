using Game.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Game.UI
{
    [RequireComponent(typeof(UIAnimator))]
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

        private UIAnimator uiAnimator;

        private Dialogue CurrentDialogue
        {
            set
            {
                currentDialogue = value;

                if (currentDialogue != null)
                {
                    characterSprite.sprite = currentDialogue.characterSprite;
                    characterSpriteContainer.SetActive(currentDialogue.characterSprite != null);

                    characterName.text = currentDialogue.characterName;
                    characterNameContainer.SetActive(!string.IsNullOrEmpty(currentDialogue.characterName));

                    graphicGroup.CrossFadeColor(currentDialogue.characterColor, 0.25f, false, false);

                    text.SetText(string.Empty);
                    CoroutineManager.Instance.ExecuteAfterFrames(() =>
                    {
                        if (currentDialogue != null)
                        {
                            text.SetText(currentDialogue.text);
                        }
                    }, 1);
                }
            }
        }
        private Dialogue currentDialogue;

        private HashSet<NamedObjectButton> activeDisplays = new();

        private ObjectPool<NamedObjectButton> displayPool;

        protected override void HandleAwake()
        {
            uiAnimator = GetComponent<UIAnimator>();

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

            dialogue.onEnterState?.Invoke();

            StartCoroutine(Delay(dialogue.delayUntilShown, PostDelay));
    
            void PostDelay()
            {
                Show();
            
                CurrentDialogue = dialogue;
                if (dialogue.audioCue != null)
                {
                    SoundManager.Instance.Play(dialogue.audioCue);
                }

                InputManager.Instance.SubmitInput.OnPressEvent += OnSubmit;
                InputManager.Instance.PointerSelectInput.OnPressEvent += OnSubmit;
                
                GlobalEvents.Dialogue.DialogueStartEvent?.Invoke();
            }
        }

        private IEnumerator Delay(float delay, VoidEvent postDelay)
        {
            yield return new WaitForSeconds(delay);
            postDelay?.Invoke();
        }

        private void TryNextDialogue(Dialogue dialogue)
        {
            if (dialogue == null)
            {
                ExitDialogue();
                return;
            }

            InputManager.Instance.SubmitInput.OnPressEvent -= OnSubmit;
            InputManager.Instance.PointerSelectInput.OnPressEvent -= OnSubmit;

            dialogue.onEnterState?.Invoke();

            StartCoroutine(Delay(dialogue.delayUntilShown, PostDelay));

            void PostDelay()
            {
                CurrentDialogue = dialogue;

                InputManager.Instance.SubmitInput.OnPressEvent += OnSubmit;
                InputManager.Instance.PointerSelectInput.OnPressEvent += OnSubmit;
            
                if (dialogue.audioCue != null)
                {
                    SoundManager.Instance.Play(dialogue.audioCue);
                }
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
            uiAnimator.Show();
            HUDRoot.Instance.Hide();
        }

        private void Hide()
        {
            uiAnimator.Hide();
            HUDRoot.Instance.Show();
        }
    }
}
