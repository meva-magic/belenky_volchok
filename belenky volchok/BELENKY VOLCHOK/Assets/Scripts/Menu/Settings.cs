using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    
    private const string VOLUME_KEY = "Volume";
    private const string SOUND_KEY = "SoundOn";
    
    private void Start()
    {
        LoadSettings();
        FixInvalidVolume();
        
        // Audio listeners
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume * 100f;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (soundToggleButton != null)
            soundToggleButton.onClick.AddListener(ToggleSound);
        
        // Language listeners - direct call to LanguageManager
        if (englishButton != null)
            englishButton.onClick.AddListener(() => LanguageManager.Instance?.SetLanguage(0));
        
        if (russianButton != null)
            russianButton.onClick.AddListener(() => LanguageManager.Instance?.SetLanguage(1));
        
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
    
    private void ApplyAudioSettings()
    {
        AudioListener.volume = isSoundOn ? currentVolume : 0f;
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
        PlayerPrefs.SetFloat(VOLUME_KEY, currentVolume);
        PlayerPrefs.SetInt(SOUND_KEY, isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    private void LoadSettings()
    {
        currentVolume = PlayerPrefs.GetFloat(VOLUME_KEY, 1f);
        isSoundOn = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
    }
}
