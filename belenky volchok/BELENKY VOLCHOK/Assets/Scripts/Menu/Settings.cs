using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;

public class SettingsPanel : MonoBehaviour
{
    [Header("Audio UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button soundToggleButton;
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private TextMeshProUGUI volumePercentText;
    
    [Header("Language UI")]
    [SerializeField] private Button englishButton;
    [SerializeField] private Button russianButton;
    
    [Header("Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    
    private float currentVolume = 1f;
    private bool isSoundOn = true;
    private int englishLocaleIndex = 0;
    private int russianLocaleIndex = 1;
    
    private void Start()
    {
        LoadSettings();
        FixInvalidVolume();
        
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume * 100f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (soundToggleButton != null)
            soundToggleButton.onClick.AddListener(ToggleSound);
        
        if (englishButton != null)
            englishButton.onClick.AddListener(() => SetLanguage(englishLocaleIndex));
        
        if (russianButton != null)
            russianButton.onClick.AddListener(() => SetLanguage(russianLocaleIndex));
        
        UpdateUI();
        ApplyAudioSettings();
    }
    
    private void FixInvalidVolume()
    {
        if (currentVolume > 1f || float.IsNaN(currentVolume) || currentVolume < 0f)
        {
            currentVolume = 1f;
            SaveSettings();
        }
    }
    
    private void OnVolumeChanged(float sliderValue)
    {
        currentVolume = sliderValue / 100f;
        UpdateVolumeDisplay();
        ApplyAudioSettings();
        SaveSettings();
    }
    
    private void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        UpdateUI();
        ApplyAudioSettings();
        SaveSettings();
    }
    
    private void SetLanguage(int localeIndex)
    {
        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeIndex];
        PlayerPrefs.SetInt("SelectedLanguage", localeIndex);
        PlayerPrefs.Save();
    }
    
    private void ApplyAudioSettings()
    {
        float finalVolume = isSoundOn ? currentVolume : 0f;
        AudioListener.volume = finalVolume;
    }
    
    private void UpdateUI()
    {
        if (volumeSlider != null)
        {
            volumeSlider.interactable = isSoundOn;
            volumeSlider.value = currentVolume * 100f;
        }
        
        if (soundButtonImage != null)
            soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        
        UpdateVolumeDisplay();
    }
    
    private void UpdateVolumeDisplay()
    {
        if (volumePercentText != null)
        {
            int percent = Mathf.RoundToInt(currentVolume * 100);
            volumePercentText.text = $"{percent}%";
        }
    }
    
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Volume", currentVolume);
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadSettings()
    {
        currentVolume = PlayerPrefs.GetFloat("Volume", 1f);
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
    }
}
