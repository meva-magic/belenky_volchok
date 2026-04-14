using UnityEngine;
using UnityEngine.UI;

public class SanityBar : MonoBehaviour
{
    public Slider sanitySlider;
    public Slider damegeSliderDelay;
    
    private float lerpSpeed = 0.03f;
    
    public static SanityBar instance;
    
    private void Awake()
    {
        instance = this;
    }
    
    private void Start()
    {
        sanitySlider.value = 1;
        damegeSliderDelay.value = 1;
    }
    
    private void Update()
    {
        float sanityValue = PlayerSanity.instance.sanity / 100;
        
        if (sanitySlider.value != sanityValue)
        {
            sanitySlider.value = sanityValue;
        }
        
        if (sanitySlider.value != damegeSliderDelay.value)
        {
            damegeSliderDelay.value = Mathf.Lerp(damegeSliderDelay.value, sanityValue, lerpSpeed);
        }
    }
}