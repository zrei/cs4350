using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.UI;
using UnityEngine.UI;
using TMPro;

public class MoralityDisplay : MonoBehaviour
{
    [SerializeField]
    private float displayLength = 485;

    [SerializeField]
    private ProgressBar goodBar;
    [SerializeField]
    private GraphicGroup goodGraphicGroup;
    [SerializeField]
    private ProgressBar evilBar;
    [SerializeField]
    private GraphicGroup evilGraphicGroup;
    [SerializeField]
    private GraphicGroup marker;
    [SerializeField]
    private TextMeshProUGUI text;

    private float Morality
    {
        get => morality;
        set
        {
            value = Mathf.Clamp(value, -1, 1);

            IEnumerator Animate()
            {
                var startMorality = morality;
                var targetMorality = value;

                var markerPos = marker.transform.localPosition;
                var t = 0f;
                var duration = 0.25f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    morality = Mathf.Lerp(startMorality, targetMorality, t / duration);

                    goodBar.SetValue(Mathf.Max(0, morality), 1f, 0f);
                    evilBar.SetValue(Mathf.Max(0, -morality), 1f, 0f);
                    markerPos.x = morality * displayLength;
                    marker.transform.localPosition = markerPos;
                    marker.color = Color.Lerp(evilGraphicGroup.color, goodGraphicGroup.color, morality / 2f + 0.5f);
                    text.text = $"{Mathf.RoundToInt(morality * 100)}<sprite name=\"Morality\" tint>";
                    yield return null;
                }

                morality = targetMorality;

                goodBar.SetValue(Mathf.Max(0, morality), 1f, 0f);
                evilBar.SetValue(Mathf.Max(0, -morality), 1f, 0f);
                markerPos.x = morality * displayLength;
                marker.transform.localPosition = markerPos;
                marker.color = Color.Lerp(evilGraphicGroup.color, goodGraphicGroup.color, morality / 2f + 0.5f);
                text.text = $"{Mathf.RoundToInt(morality * 100)}<sprite name=\"Morality\" tint>";
            }

            StopAllCoroutines();
            StartCoroutine(Animate());
        }
    }
    [SerializeField]
    [SerializeProperty("Morality")]
    private float morality;

    private Animator animator;
    private CanvasGroup canvasGroup;
    private bool isHidden;

    private void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        isHidden = true;

        GlobalEvents.Morality.MoralityChangeEvent += OnMoralityChange;
        GlobalEvents.Scene.LevelSceneLoadedEvent += Show;
        GlobalEvents.Scene.BattleSceneLoadedEvent += Hide;
        GlobalEvents.Level.ReturnFromLevelEvent += Hide;

        Morality = MoralityManager.Instance.CurrMoralityPercentage;
    }

    private void OnDestroy()
    {
        GlobalEvents.Scene.LevelSceneLoadedEvent -= Show;
        GlobalEvents.Scene.BattleSceneLoadedEvent -= Hide;
        GlobalEvents.Level.ReturnFromLevelEvent -= Hide;
    }

    private void OnMoralityChange(int value)
    {
        Morality = MoralityManager.Instance.CurrMoralityPercentage;
    }

    private void Show()
    {
        if (!isHidden) return;

        isHidden = false;
        animator.enabled = true;
        animator.Play(UIConstants.ShowAnimHash);
    }

    private void Hide()
    {
        if (isHidden) return;

        isHidden = true;
        animator.enabled = true;
        animator.Play(UIConstants.HideAnimHash);
    }

    private void OnAnimationFinish()
    {
        animator.enabled = false;
    }
}
