using Game;
using Game.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyActionForecastDisplay : MonoBehaviour
{
    #region Asset References
    [SerializeField]
    private Sprite attackIcon;
    [SerializeField]
    private Sprite moveIcon;
    [SerializeField]
    private Sprite passIcon;
    #endregion

    [SerializeField]
    private Vector3 worldOffset;

    [SerializeField]
    private Image icon;

    private EnemyUnit TrackedUnit
    {
        get => trackedUnit;
        set
        {
            if (trackedUnit == value) return;

            if (trackedUnit != null)
            {
                trackedUnit.OnDecideAction -= OnDecideAction;
            }

            trackedUnit = value;
            if (trackedUnit != null)
            {
                trackedUnit.OnDecideAction += OnDecideAction;
                BeginFollow();
            }
        }
    }
    private EnemyUnit trackedUnit;

    private Animator animator;
    private CanvasGroup canvasGroup;

    private bool isHidden;

    private Coroutine followCoroutine;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;

        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0;
        isHidden = true;

        GlobalEvents.Battle.BattleEndEvent += OnBattleEnd;
    }

    private void OnBattleEnd(UnitAllegiance unitAllegiance)
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;

        TrackedUnit = null;
        Hide();
    }

    private void OnDestroy()
    {
        GlobalEvents.Battle.BattleEndEvent -= OnBattleEnd;
    }

    private void Start()
    {
        // avoid null error in the level node scene
        if (!CameraManager.IsReady)
        {
            Destroy(gameObject);
            return;
        }

        var parentUnit = transform.parent.GetComponent<EnemyUnit>();
        if (parentUnit == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.SetParent(parentUnit.transform.parent);
        TrackedUnit = parentUnit;
    }

    private void BeginFollow()
    {
        if (trackedUnit == null || !trackedUnit) return;

        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
        followCoroutine = StartCoroutine(FollowPosition(trackedUnit.transform));
    }

    private IEnumerator FollowPosition(Transform t)
    {
        while (t && t != null)
        {
            //transform.localPosition = WorldHUDManager.Instance.WorldToHUDSpace(t.position + worldOffset);
            transform.position = t.position + worldOffset;
            var rot = CameraManager.Instance.MainCamera.transform.rotation;
            rot.x = 0;
            rot.z = 0;
            transform.rotation = rot;
            yield return null;
        }

        // if this is reached, the tracked transform was destroyed
        trackedUnit = null;
        Hide();
    }

    private void OnDecideAction(EnemyActionSO action)
    {
        if (action != null)
        {
            Show();
            if (action is EnemyActiveSkillActionSO activeSkillAction)
            {
                icon.sprite = attackIcon;
                icon.CrossFadeColor(Color.red, 0.5f, false, true);
            }
            else if (action is EnemyMoveActionSO moveAction)
            {
                icon.sprite = moveIcon;
                icon.CrossFadeColor(Color.blue, 0.5f, false, true);
            }
            else
            {
                icon.sprite = passIcon;
                icon.CrossFadeColor(Color.white, 0.5f, false, true);
            }
        }
        else
        {
            Hide();
        }
    }

    public void Show()
    {
        if (!isHidden) return;

        isHidden = false;
        animator.enabled = true;
        animator.Play(UIConstants.ShowAnimHash);
    }

    public void Hide()
    {
        if (isHidden) return;

        isHidden = true;
        animator.enabled = true;
        animator.CrossFade(UIConstants.HideAnimHash, 0.1f);
    }

    private void OnAnimationFinish()
    {
        animator.enabled = !isHidden;
        canvasGroup.interactable = !isHidden;
        canvasGroup.blocksRaycasts = !isHidden;
    }
}
