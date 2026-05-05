using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance { get; private set; }
    
    private const string LANGUAGE_KEY = "SelectedLanguage";
    private bool isInitialized = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private IEnumerator Start()
    {
        yield return LocalizationSettings.InitializationOperation;
        isInitialized = true;
        LoadSavedLanguage();
    }
    
    public bool IsInitialized => isInitialized;
    
    public void SetLanguage(int localeIndex)
    {
        if (!isInitialized)
        {
            Debug.LogWarning("Localization not initialized yet");
            return;
        }
        
        if (localeIndex < 0 || localeIndex >= LocalizationSettings.AvailableLocales.Locales.Count)
        {
            Debug.LogError($"Invalid locale index: {localeIndex}");
            return;
        }
        
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
        PlayerPrefs.SetInt(LANGUAGE_KEY, localeIndex);
        PlayerPrefs.Save();
    }
    
    private void LoadSavedLanguage()
    {
        int savedIndex = PlayerPrefs.GetInt(LANGUAGE_KEY, 0);
        SetLanguage(savedIndex);
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
