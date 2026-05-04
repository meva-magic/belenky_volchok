using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    [Header("Language Buttons")]
    [SerializeField] private Button englishButton;
    [SerializeField] private Button russianButton;
    
    private int englishLocaleIndex = 0;
    private int russianLocaleIndex = 1;
    private bool isInitialized = false;
    
    private IEnumerator Start()
    {
        // Wait for localization to fully initialize
        yield return LocalizationSettings.InitializationOperation;
        isInitialized = true;
        
        // Setup button listeners
        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(englishLocaleIndex));
        
        if (russianButton != null)
            russianButton.onClick.AddListener(() => SetLanguage(russianLocaleIndex));
        
        // Now safe to load saved language
        LoadSavedLanguage();
    }
    
    private void SetLanguage(int localeIndex)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Cannot change language - localization not initialized yet");
            return;
        }
        
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
        
        // Save preference
        PlayerPrefs.SetInt("SelectedLanguage", localeIndex);
        PlayerPrefs.Save();
    }
    
    private void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey("SelectedLanguage"))
        {
            int savedIndex = PlayerPrefs.GetInt("SelectedLanguage");
            SetLanguage(savedIndex);
        }
    }
    
    private void OnDestroy()
    {
        // Clean up listeners
        if (englishButton != null)
            englishButton.onClick.RemoveAllListeners();
        
        if (russianButton != null)
            russianButton.onClick.RemoveAllListeners();
    }
}
