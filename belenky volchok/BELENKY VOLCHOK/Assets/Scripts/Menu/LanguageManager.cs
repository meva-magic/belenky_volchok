using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LanguageManager : MonoBehaviour
{
    [Header("Language Buttons")]
    [SerializeField] private Button englishButton;
    [SerializeField] private Button russianButton;
    
    [Header("Button Visuals (Optional)")]
    [SerializeField] private Image englishButtonImage;
    [SerializeField] private Image russianButtonImage;
    [SerializeField] private Sprite selectedSprite;
    [SerializeField] private Sprite normalSprite;
    
    private int englishLocaleIndex = 0;
    private int russianLocaleIndex = 1;
    
    private void Start()
    {
        // Setup button listeners
        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(englishLocaleIndex));
        
        if (russianButton != null)
            russianButton.onClick.AddListener(() => SetLanguage(russianLocaleIndex));
        
        // Update button visuals based on current language
        UpdateButtonVisuals();
    }
    
    private void SetLanguage(int localeIndex)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
        UpdateButtonVisuals();
        
        // Save preference
        PlayerPrefs.SetInt("SelectedLanguage", localeIndex);
        PlayerPrefs.Save();
    }
    
    private void UpdateButtonVisuals()
    {
        if (selectedSprite == null || normalSprite == null) return;
        
        int currentIndex = LocalizationSettings.AvailableLocales.Locales.IndexOf(LocalizationSettings.SelectedLocale);
        
        if (englishButtonImage != null)
            englishButtonImage.sprite = currentIndex == englishLocaleIndex ? selectedSprite : normalSprite;
        
        if (russianButtonImage != null)
            russianButtonImage.sprite = currentIndex == russianLocaleIndex ? selectedSprite : normalSprite;
    }
    
    // Call this if you want to load saved language preference
    public void LoadSavedLanguage()
    {
        if (PlayerPrefs.HasKey("SelectedLanguage"))
        {
            int savedIndex = PlayerPrefs.GetInt("SelectedLanguage");
            SetLanguage(savedIndex);
        }
    }
}
