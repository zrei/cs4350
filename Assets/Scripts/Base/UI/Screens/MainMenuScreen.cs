using Game.UI;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Bypasses the UI system for now
/// </summary>
public class MainMenuScreen : MonoBehaviour
{
    [SerializeField] private RawImage m_BottomGlow;
    [SerializeField] private float m_TextureAnimSpeed = 0.1f;
    [SerializeField] private NamedObjectButton m_NewGameButton;
    [SerializeField] private NamedObjectButton m_ContinueButton;
    [SerializeField] private NamedObjectButton m_CreditsButton;
    [SerializeField] private NamedObjectButton m_OptionsButton;
    [SerializeField] private NamedObjectButton m_QuitButton;

    [Header("Screens")]
    [SerializeField] private BaseUIScreen m_CreditsScreen;
    [SerializeField] private BaseUIScreen m_OptionsScreen;

    private void Awake()
    {
        HandleDependencies();
        m_CreditsScreen.Initialize();
    }

    private void Start()
    {
        m_NewGameButton.onSubmit.AddListener(B_StartNewGame);
        m_ContinueButton.onSubmit.AddListener(B_Continue);
        m_QuitButton.onSubmit.AddListener(B_QuitGame);
        m_CreditsButton.onSubmit.AddListener(B_Credits);
        m_OptionsButton.onSubmit.AddListener(B_Options);
    }

    private void OnDestroy()
    {
        RemoveListeners();
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
        RemoveListeners();
        SaveManager.Instance.CreateNewSave();
        GoToWorldMap();
    }

    private void B_Continue()
    {
        RemoveListeners();
        GoToWorldMap();
    }

    private void GoToWorldMap()
    {
        GlobalEvents.MainMenu.OnBeginLoadWorldMap?.Invoke();
        GameSceneManager.Instance.LoadWorldMapScene();
    }

    private void B_QuitGame()
    {
        RemoveListeners();
        Application.Quit();
    }

    private void B_Credits()
    {
        m_CreditsScreen.Show();
    }

    private void B_Options()
    {
        m_OptionsScreen.Show();
    }

    private void Update()
    {
        var rect = m_BottomGlow.uvRect;
        rect.x = Time.time * m_TextureAnimSpeed;
        m_BottomGlow.uvRect = rect;
    }
    #endregion

    #region Btn Listeners
    private void RemoveListeners()
    {
        m_NewGameButton.onSubmit.RemoveListener(B_StartNewGame);
        m_ContinueButton.onSubmit.RemoveListener(B_Continue);
        m_QuitButton.onSubmit.RemoveListener(B_QuitGame);
    }
    #endregion
}
