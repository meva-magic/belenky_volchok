using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmMenuPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button confirmYesButton;
    [SerializeField] private Button confirmNoButton;
    
    private bool isPaused = false;
    
    private void Start()
    {
        // Hide panels at start
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(false);
        
        // Setup button listeners
        if (continueButton != null)
            continueButton.onClick.AddListener(ResumeGame);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);
        if (menuButton != null)
            menuButton.onClick.AddListener(ShowConfirmMenu);
        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);
        if (confirmYesButton != null)
            confirmYesButton.onClick.AddListener(ConfirmGoToMenu);
        if (confirmNoButton != null)
            confirmNoButton.onClick.AddListener(CancelConfirmMenu);
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused && settingsPanel.activeSelf)
                CloseSettings();
            else if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }
    
    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(false);
    }
    
    private void OpenSettings()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }
    
    private void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    private void ShowConfirmMenu()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(true);
    }
    
    private void CancelConfirmMenu()
    {
        if (confirmMenuPanel != null)
            confirmMenuPanel.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(true);
    }
    
    private void ConfirmGoToMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    
    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ResumeGame);
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenSettings);
        if (menuButton != null)
            menuButton.onClick.RemoveListener(ShowConfirmMenu);
        if (backButton != null)
            backButton.onClick.RemoveListener(CloseSettings);
        if (confirmYesButton != null)
            confirmYesButton.onClick.RemoveListener(ConfirmGoToMenu);
        if (confirmNoButton != null)
            confirmNoButton.onClick.RemoveListener(CancelConfirmMenu);
    }
}