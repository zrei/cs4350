using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Game.UI
{
    public class ReclassPanel : MonoBehaviour
    {
        [Header("Tab")]
        [SerializeField] private NamedObjectButton m_OverviewButton;

        [Header("UI References")]
        [SerializeField] private FormattedTextDisplay m_PathGroupTitle;
        [SerializeField] private NamedObjectButton m_ReclassButton;
        [SerializeField] private FormattedTextDisplay m_ClassTitle;
        [SerializeField] private TextMeshProUGUI m_ClassDescription;        

        [Header("Class List")]
        [SerializeField] private Transform m_ClassButtonParent;
        [SerializeField] private NamedObjectButton m_ClassButtonPrefab;

        [Header("Components")]
        [SerializeField] private ActiveSkillDisplay_Reclass m_ActiveSkillDisplay;
        [SerializeField] private PassiveSkillDisplay_Reclass m_PassiveSkillDisplay;
        [SerializeField] private ConditionsDisplay_Reclass m_ConditionDisplay;
        [SerializeField] private CanvasGroup m_CanvasGroup;

        [HideInInspector]
        public event UnityAction OnOverviewEvent;

        private int m_CurrSelectedClassIndex;
        private int m_CurrEquippedClassIndex;

        private PlayerCharacterData m_CurrCharacterData;

        private void Start()
        {
            m_OverviewButton.onSubmit.RemoveAllListeners();
            m_OverviewButton.onSubmit.AddListener(OnOverviewEvent);

            m_ReclassButton.onSubmit.RemoveAllListeners();
            m_ReclassButton.onSubmit.AddListener(B_Reclass);
        }

        public void SetDisplay(PlayerCharacterData playerCharacterData)
        {
            m_CurrCharacterData = playerCharacterData;
            PathGroupSO pathGroupSO = playerCharacterData.m_BaseData.m_PathGroup;
            m_PathGroupTitle?.SetValue(pathGroupSO.m_PathName);
            ResetDisplay();

            for (int i = 0; i < playerCharacterData.NumClasses; ++i)
            {
                NamedObjectButton classButtonInstance = Instantiate(m_ClassButtonPrefab, m_ClassButtonParent);

                PlayerClassSO classSO = pathGroupSO.GetClass(i);
                classButtonInstance.icon.sprite = classSO.m_Icon;
                classButtonInstance.nameText.text = classSO.m_ClassName;
                int cachedIndex = i;
                classButtonInstance.onSubmit.AddListener(() => SelectClass(pathGroupSO.m_PathClasses[cachedIndex], cachedIndex, !playerCharacterData.IsClassUnlocked(cachedIndex)));            
            }

            ToggleShown(false);
        }

        private void SelectClass(PathClass pathClass, int classIndex, bool isLocked)
        {
            m_CurrSelectedClassIndex = classIndex;

            PlayerClassSO playerClassSO = pathClass.m_Class;

            m_ClassDescription.text = playerClassSO.m_ClassDescription;

            m_ReclassButton.interactable = !isLocked && m_CurrEquippedClassIndex != classIndex;
            m_ActiveSkillDisplay.SetDisplay(m_CurrCharacterData, playerClassSO);
            m_PassiveSkillDisplay.SetDisplay(m_CurrCharacterData, playerClassSO);
            m_ConditionDisplay.SetDisplay(pathClass);

            SetClassTitle(isLocked);
    
            ToggleShown(true);

            GlobalEvents.CharacterManagement.OnPreviewReclass?.Invoke(playerClassSO);
        }

        private void SetClassTitle(bool isLocked)
        {
            string className = m_CurrCharacterData.m_BaseData.m_PathGroup.GetClass(m_CurrSelectedClassIndex).m_ClassName;
            if (isLocked)
            {
                m_ClassTitle?.SetValue(className + " (LOCKED)");
            }
            else if (m_CurrEquippedClassIndex == m_CurrSelectedClassIndex)
            {
                m_ClassTitle?.SetValue(className + " (CURRENT)");
            }
            else
            {
                m_ClassTitle?.SetValue(className);
            }
            
        }

        private void B_Reclass()
        {
            m_CurrCharacterData.Reclass(m_CurrSelectedClassIndex);
            m_CurrEquippedClassIndex = m_CurrSelectedClassIndex;
            GlobalEvents.CharacterManagement.OnReclass?.Invoke();
            m_ReclassButton.interactable = false;
            SetClassTitle(false);

            if (m_CurrCharacterData.IsLord)
                GlobalEvents.CharacterManagement.OnLordUpdate?.Invoke();
        }

        private void ResetDisplay()
        {
            foreach (Transform transform in m_ClassButtonParent)
            {
                Destroy(transform.gameObject);
            }
        }

        private void ToggleShown(bool shown)
        {
            m_CanvasGroup.alpha = shown ? 1f : 0f;
            m_CanvasGroup.interactable = shown;
            m_CanvasGroup.blocksRaycasts = shown;
        }
    }
}