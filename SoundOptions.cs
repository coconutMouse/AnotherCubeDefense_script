using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine;

public class SoundOptions : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider BgmSlider;
    public Slider SfxSlider;

    public void SetBgmVolme()
    {
        audioMixer.SetFloat("BGM", Mathf.Log10(BgmSlider.value) * 20);
    }

    public void SetSFXVolme()
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(SfxSlider.value) * 20);
    }
    private void Start()
    {
        float bgmSliderValue;
        audioMixer.GetFloat("BGM", out bgmSliderValue);
        BgmSlider.value = Mathf.Pow(10, bgmSliderValue / 20f);
        float sfxSliderValue;
        audioMixer.GetFloat("SFX", out sfxSliderValue);
        SfxSlider.value = Mathf.Pow(10, sfxSliderValue / 20f);
    }
}