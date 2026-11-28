using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WaterMeter waterMeter;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("HUD Elements")]
    [SerializeField] private Image waterFillImage;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private Color fullWaterColor = new Color(0.15f, 0.72f, 1f, 1f);
    [SerializeField] private Color lowWaterColor = new Color(1f, 0.3f, 0.2f, 1f);
    [SerializeField] private float lowWaterThreshold = 0.3f;

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private InputSystem inputActions;
    private bool isPaused;
    private bool isEnded;

    private void Awake()
    {
        Time.timeScale = 1f;
        inputActions = new InputSystem();
    }

    private void OnEnable()
    {
        inputActions.UI.Enable();
        inputActions.UI.Cancel.performed += OnCancelPerformed;

        if (waterMeter != null) waterMeter.OnWaterChanged += UpdateWaterUI;
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged += UpdateLivesUI;
            playerHealth.OnGameOver += ShowGameOver;
            playerHealth.OnLevelCompleted += ShowWin;
        }
    }

    private void OnDisable()
    {
        inputActions.UI.Cancel.performed -= OnCancelPerformed;
        inputActions.UI.Disable();

        if (waterMeter != null) waterMeter.OnWaterChanged -= UpdateWaterUI;
        if (playerHealth != null)
        {
            playerHealth.OnLivesChanged -= UpdateLivesUI;
            playerHealth.OnGameOver -= ShowGameOver;
            playerHealth.OnLevelCompleted -= ShowWin;
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        inputActions?.Dispose();
    }

    private void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    public void TogglePause()
    {
        if (isEnded)
        {
            return;
        }

        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(isPaused);
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        TogglePause();
    }

    private void UpdateWaterUI(float normalized)
    {
        if (waterFillImage == null)
        {
            return;
        }

        waterFillImage.fillAmount = normalized;
        waterFillImage.color = normalized <= lowWaterThreshold
            ? Color.Lerp(lowWaterColor, fullWaterColor, normalized / Mathf.Max(0.01f, lowWaterThreshold))
            : fullWaterColor;
    }

    private void UpdateLivesUI(int currentLives, int maxLives)
    {
        if (livesText != null)
        {
            livesText.text = $"Lives: {currentLives} / {maxLives}";
        }
    }

    private void ShowGameOver()
    {
        isEnded = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    private void ShowWin()
    {
        isEnded = true;
        isPaused = false;
        Time.timeScale = 0f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(true);
    }
}
