using UnityEngine;
using UnityEngine.UI;
using Input = UnityEngine.Input;



[RequireComponent(typeof(Controller2D))]



public class OxygenBar : MonoBehaviour
{
    public Slider oxygenSlider;
    public RectTransform oxygenSliderTransform;
    public Slider easeOxygenSlider;
    public RectTransform easeOxygenSliderTransform;

    float initialMaxOxygen;
    float initialWidth;

    public float maxOxygen = 100f;
    public float currentOxygen = 100;
    [SerializeField] float lerpSpeed = 0.05f;



    [SerializeField] Image OxygenFillImage;
    Color originalOxygenColor;
    Color darkestOxygenColor = new Color32(0x00, 0x40, 0x9F, 0xFF);
    float oxygenPercentage;
    Color currentColorOfOxygen;



    float lastFrameOxygen;



    [SerializeField] float timeToNoOxygenLeftComparedTo100;
    float decreaseSpeedOfOxygen;



    private void Start()
    {
        currentOxygen = maxOxygen;
        lastFrameOxygen = currentOxygen;



        initialMaxOxygen = maxOxygen;
        initialWidth = oxygenSliderTransform.sizeDelta.x;



        UpdateOxygenUI();



        decreaseSpeedOfOxygen = 100 / timeToNoOxygenLeftComparedTo100;



        originalOxygenColor = OxygenFillImage.color;
    }



    void UpdateOxygenUI()
    {
        oxygenSlider.maxValue = maxOxygen;
        oxygenSlider.value = currentOxygen;
        easeOxygenSlider.maxValue = maxOxygen;
        easeOxygenSlider.value = currentOxygen;



        float scaleFactor = maxOxygen / initialMaxOxygen;
        oxygenSliderTransform.sizeDelta = new Vector2(initialWidth * scaleFactor, oxygenSliderTransform.sizeDelta.y);
        easeOxygenSliderTransform.sizeDelta = new Vector2(initialWidth * scaleFactor, oxygenSliderTransform.sizeDelta.y);
    }



    private void Update()
    {
        currentOxygen -= decreaseSpeedOfOxygen * Time.deltaTime;



        if (currentOxygen <= 0)
        {
            currentOxygen = 0;
        }
        if (currentOxygen >= maxOxygen)
        {
            currentOxygen = maxOxygen;
        }



        if (oxygenSlider.value != currentOxygen)
        {
            oxygenSlider.value = currentOxygen;
        }



        if(easeOxygenSlider.value - currentOxygen > decreaseSpeedOfOxygen + 0.5f)
        {
            easeOxygenSlider.value = Mathf.Lerp(easeOxygenSlider.value, currentOxygen, lerpSpeed);
        }
        else
        {
            easeOxygenSlider.value = oxygenSlider.value;
        }



        oxygenPercentage = Mathf.Clamp01(currentOxygen / maxOxygen);

        currentColorOfOxygen = Color.Lerp(darkestOxygenColor, originalOxygenColor, oxygenPercentage);

        OxygenFillImage.color = currentColorOfOxygen;
        OxygenFillImage.fillAmount = oxygenPercentage;



        lastFrameOxygen = currentOxygen;
    }
}
