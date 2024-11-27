using System.Collections;
using UnityEngine;
using Game.UI;
using TMPro;

[RequireComponent(typeof(UIAnimator))]
public class MoralityDisplay : MonoBehaviour
{
    [SerializeField]
    private float displayLength = 512;
    [SerializeField]
    private float displayLengthClamp = 507;
    [SerializeField]
    private Gradient colorGradient;

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
                var morality01 = morality / 2f + 0.5f;

                var markerPos = marker.transform.localPosition;
                var t = 0f;
                var duration = 0.25f;
                while (t < duration)
                {
                    t += Time.deltaTime;
                    morality = Mathf.Lerp(startMorality, targetMorality, t / duration);
                    morality01 = morality / 2f + 0.5f;

                    goodBar.SetValue(Mathf.Max(0, morality), 1f, 0f);
                    evilBar.SetValue(Mathf.Max(0, -morality), 1f, 0f);
                    markerPos.x = Mathf.Clamp(morality * displayLength, -displayLengthClamp, displayLengthClamp);
                    marker.transform.localPosition = markerPos;
                    marker.color = colorGradient.Evaluate(morality01);
                    text.text = $"{Mathf.RoundToInt(morality * 100)}<sprite name=\"Morality\" tint>";
                    yield return null;
                }

                morality = targetMorality;
                morality01 = morality / 2f + 0.5f;

                goodBar.SetValue(Mathf.Max(0, morality), 1f, 0f);
                evilBar.SetValue(Mathf.Max(0, -morality), 1f, 0f);
                markerPos.x = Mathf.Clamp(morality * displayLength, -displayLengthClamp, displayLengthClamp); ;
                marker.transform.localPosition = markerPos;
                marker.color = colorGradient.Evaluate(morality01);
                text.text = $"{Mathf.RoundToInt(morality * 100)}<sprite name=\"Morality\" tint>";
            }

            StopAllCoroutines();
            StartCoroutine(Animate());
        }
    }
    [SerializeField]
    [SerializeProperty("Morality")]
    private float morality;

    private UIAnimator uiAnimator;

    private void Start()
    {
        uiAnimator = GetComponent<UIAnimator>();

        GlobalEvents.Morality.MoralityChangeEvent += OnMoralityChange;
        GlobalEvents.Morality.MoralitySetEvent += OnMoralitySet;
        GlobalEvents.Level.LevelEndEvent += Hide;
        MoralityManager.OnReady += OnMoralityReady;

        Show();
 
        //Morality = MoralityManager.Instance.CurrMoralityPercentage;
    }

    private void OnMoralityReady()
    {
        Morality = MoralityManager.Instance.CurrMoralityPercentage;
    }

    private void OnDestroy()
    {

        GlobalEvents.Morality.MoralityChangeEvent -= OnMoralityChange;
        GlobalEvents.Morality.MoralitySetEvent -= OnMoralitySet;
        GlobalEvents.Level.LevelEndEvent -= Hide;
        MoralityManager.OnReady -= OnMoralityReady;
    }

    private void OnMoralitySet(int value)
    {
        StopAllCoroutines();
        morality = value;
        var morality01 = morality / 2f + 0.5f;
        goodBar.SetValue(Mathf.Max(0, morality), 1f, 0f);
        evilBar.SetValue(Mathf.Max(0, -morality), 1f, 0f);
        marker.transform.localPosition = new Vector3(
            Mathf.Clamp(morality * displayLength, -displayLengthClamp, displayLengthClamp),
            marker.transform.localPosition.y,
            marker.transform.localPosition.z);
        marker.color = colorGradient.Evaluate(morality01);
        text.text = $"{Mathf.RoundToInt(morality * 100)}<sprite name=\"Morality\" tint>";
    }

    private void OnMoralityChange(int value)
    {
        Morality = MoralityManager.Instance.CurrMoralityPercentage;
    }

    private void Show()
    {
        uiAnimator.Show();
    }

    private void Hide()
    {
        uiAnimator.Hide();
    }
}
