using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject confirmExitPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button confirmExitYesButton;
    [SerializeField] private Button confirmExitNoButton;
    
    [Header("Scene Names")]
    [SerializeField] private string prologueSceneName = "Prologue";
    //[SerializeField] private string gameSceneName = "GameScene";
    
    private void Start()
    {
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (settingsButton != null) settingsButton.onClick.AddListener(OpenSettings);
        if (exitButton != null) exitButton.onClick.AddListener(OpenConfirmExit);
        if (backButton != null) backButton.onClick.AddListener(CloseSettings);
        if (confirmExitYesButton != null) confirmExitYesButton.onClick.AddListener(ConfirmExit);
        if (confirmExitNoButton != null) confirmExitNoButton.onClick.AddListener(CloseConfirmExit);
        
        ShowMainPanel();
        Time.timeScale = 1f;
    }
    
    private void StartGame()
    {
        // Load prologue scene first
        UnityEngine.SceneManagement.SceneManager.LoadScene(prologueSceneName);
    }
    
    private void OpenSettings()
    {
        settingsPanel.SetActive(true);
        confirmExitPanel.SetActive(false);
    }
    
    private void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }
    
    private void OpenConfirmExit()
    {
        confirmExitPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
    
    private void CloseConfirmExit()
    {
        confirmExitPanel.SetActive(false);
    }
    
    private void ConfirmExit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    private void ShowMainPanel()
    {
        settingsPanel.SetActive(false);
        confirmExitPanel.SetActive(false);
    }
}
