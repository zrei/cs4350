using Game.UI;
using UnityEngine;

/// <summary>
/// Bypasses the UI system for now
/// </summary>
public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private NamedObjectButton m_NewGameButton;
    [SerializeField] private NamedObjectButton m_ContinueButton;
    [SerializeField] private NamedObjectButton m_QuitButton;

    private void Awake()
    {
        HandleDependencies();
    }

    private void Start()
    {
        m_NewGameButton.onSubmit.AddListener(B_StartNewGame);
        m_ContinueButton.onSubmit.AddListener(B_Continue);
        m_QuitButton.onSubmit.AddListener(B_QuitGame);
    }

    private void OnDestroy()
    {
        m_NewGameButton.onSubmit.RemoveListener(B_StartNewGame);
        m_ContinueButton.onSubmit.RemoveListener(B_Continue);
        m_QuitButton.onSubmit.RemoveListener(B_QuitGame);
    }

    private void HandleDependencies()
    {
        if (!SaveManager.IsReady)
        {
            SaveManager.OnReady += HandleDependencies;
            return;
        }

        SaveManager.OnReady -= HandleDependencies;

        m_ContinueButton.interactable = SaveManager.Instance.HasSave;
    }

    #region Btn Callbacks
    private void B_StartNewGame()
    {
        SaveManager.Instance.CreateNewSave();
        GameSceneManager.Instance.LoadWorldMapScene();
    }

    private void B_Continue()
    {
        GameSceneManager.Instance.LoadWorldMapScene();
    }

    private void B_QuitGame()
    {
        Application.Quit();
    }
    #endregion
}