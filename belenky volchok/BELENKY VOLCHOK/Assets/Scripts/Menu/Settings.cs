using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button soundToggleButton;
    [SerializeField] private Image soundButtonImage;
    [SerializeField] private TextMeshProUGUI volumePercentText;
    
    [Header("Sprites")]
    [SerializeField] private Sprite soundOnSprite;
    [SerializeField] private Sprite soundOffSprite;
    
    private float currentVolume = 0.75f;
    private bool isSoundOn = true;
    
    private void Start()
    {
        LoadSettings();
        
        if (volumeSlider != null)
        {
            volumeSlider.value = currentVolume;
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        }
        
        if (soundToggleButton != null)
            soundToggleButton.onClick.AddListener(ToggleSound);
        
        UpdateUI();
        ApplyAudioSettings();
    }
    
    private void OnVolumeChanged(float volume)
    {
        currentVolume = volume;
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
        float finalVolume = isSoundOn ? currentVolume : 0f;
        AudioListener.volume = finalVolume;
    }
    
    private void UpdateUI()
    {
        if (volumeSlider != null)
            volumeSlider.interactable = isSoundOn;
        
        if (soundButtonImage != null)
        {
            soundButtonImage.sprite = isSoundOn ? soundOnSprite : soundOffSprite;
        }
        
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
        currentVolume = PlayerPrefs.GetFloat("Volume", 0.75f);
        isSoundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
    }
}
