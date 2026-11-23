using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;

    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel;

    [SerializeField] private Button settingsBackButton;
    [SerializeField] private Button creditsBackButton;

    [SerializeField] private string gameplaySceneName = "MainScene";

    private void OnEnable()
    {
        if (playButton != null) playButton.onClick.AddListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (creditsButton != null) creditsButton.onClick.AddListener(OpenCredits);
        if (exitButton != null) exitButton.onClick.AddListener(ExitGame);

        if (settingsBackButton != null) settingsBackButton.onClick.AddListener(ShowMenu);
        if (creditsBackButton != null) creditsBackButton.onClick.AddListener(ShowMenu);
    }

    private void OnDisable()
    {
        if (playButton != null) playButton.onClick.RemoveListener(PlayGame);
        if (settingsButton != null) settingsButton.onClick.RemoveListener(OpenSettings);
        if (creditsButton != null) creditsButton.onClick.RemoveListener(OpenCredits);
        if (exitButton != null) exitButton.onClick.RemoveListener(ExitGame);

        if (settingsBackButton != null) settingsBackButton.onClick.RemoveListener(ShowMenu);
        if (creditsBackButton != null) creditsBackButton.onClick.RemoveListener(ShowMenu);
    }

    private void Start()
    {
        ShowMenu();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void OpenSettings()
    {
        SetPanelActive(false, true, false);
    }

    public void OpenCredits()
    {
        SetPanelActive(false, false, true);
    }

    public void ShowMenu()
    {
        SetPanelActive(true, false, false);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPanelActive(bool menu, bool settings, bool credits)
    {
        if (menuPanel != null) menuPanel.SetActive(menu);
        if (settingsPanel != null) settingsPanel.SetActive(settings);
        if (creditsPanel != null) creditsPanel.SetActive(credits);
    }
}
