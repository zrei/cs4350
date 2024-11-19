using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public enum SkillButtonState
    {
        EMPTY,
        LOCKED,
        NORMAL
    }

    public class SkillButton : ActionButton
    {
        public Color defaultIconColor = Color.red;
        public Image softGlow;
        public Image darken;
        public Image fillImage;
        public FormattedTextDisplay statusText;

        public Sprite SkillSprite
        {
            set
            {
                icon.sprite = value;
            }
        }

        public void SetFill(float fill)
        {
            Debug.Log(fill);
            fillImage.fillAmount = 1f - fill;
        }

        public void SetState(SkillButtonState skillButtonState)
        {
            interactable = skillButtonState != SkillButtonState.EMPTY;
            icon.gameObject.SetActive(skillButtonState != SkillButtonState.EMPTY);
            darken.gameObject.SetActive(skillButtonState == SkillButtonState.LOCKED);
            statusText.gameObject.SetActive(skillButtonState == SkillButtonState.LOCKED);
            softGlow.gameObject.SetActive(skillButtonState == SkillButtonState.NORMAL);
        }
    }
}
