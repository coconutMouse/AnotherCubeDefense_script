using UnityEngine;
using UnityEngine.UI;

public class ExperienceSlider : MonoBehaviour
{
    public float sliderSpeed;

    private Slider slider;
    private PlayerCharacter myPlayerCharacter;
    private float accumulatedExperiencePoint;
    private float experienceSliderValue;
    private float sliderCurrentValue;
    private int accumulatedLevel;

    private void Awake()
    {
        PlayerManager.Instance.OnMyPlayerCharacterActive += OnMyPlayerCharacterActive;
        slider = GetComponent<Slider>(); 
    }
    private void Update()
    {
        if (myPlayerCharacter == null)
            return;

        float maxExperiencePoint = myPlayerCharacter.GetMaxExperiencePoint();
        float experiencePoint = myPlayerCharacter.GetExperiencePoint();
        float currentExperienceSliderValue = (experiencePoint / maxExperiencePoint);

        if(accumulatedLevel < myPlayerCharacter.Level)
        {
            accumulatedLevel = myPlayerCharacter.Level;
            accumulatedExperiencePoint += (1f - experienceSliderValue) + currentExperienceSliderValue;
        }
        else
            accumulatedExperiencePoint += currentExperienceSliderValue - experienceSliderValue;
        experienceSliderValue = currentExperienceSliderValue;


        if (sliderCurrentValue < accumulatedExperiencePoint)
            sliderCurrentValue += Time.deltaTime * sliderSpeed;

        if (sliderCurrentValue > accumulatedExperiencePoint)
            sliderCurrentValue = accumulatedExperiencePoint;

        if (sliderCurrentValue >= 1f)
        {
            accumulatedExperiencePoint -= 1f;
            sliderCurrentValue -= 1f;
        }
        slider.value = sliderCurrentValue;

    }
    private void OnMyPlayerCharacterActive(PlayerCharacter _myPlayerCharacter)
    {
        myPlayerCharacter = _myPlayerCharacter;
    }
}
