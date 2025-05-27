using UnityEngine;
using UnityEngine.UI;
using Input = UnityEngine.Input;



[RequireComponent(typeof(Controller2D))]



public class HealthBar : MonoBehaviour
{
    public Slider healthSlider;
    public RectTransform sliderTransform;
    public Slider easeHealthSlider;
    public RectTransform easeSliderTransform;

    float initialMaxHealth;
    float initialWidth;

    public float maxHealth = 100f;
    public float currentHealth = 100;
    [SerializeField] float lerpSpeed = 0.05f;



    [SerializeField] Image healthFillImage;
    Color originalHealthColor;
    Color darkestHealthColor = new Color32(133, 0, 0, 255);
    float healthPercentage;
    Color currentColorOfHealth;



    float lastFrameHealth;



    private void Start()
    {
        currentHealth = maxHealth;
        lastFrameHealth = currentHealth;



        initialMaxHealth = maxHealth;
        initialWidth = sliderTransform.sizeDelta.x;



        UpdateHealthUI();



        originalHealthColor = healthFillImage.color;
    }



    void UpdateHealthUI()
    {
        healthSlider.maxValue = maxHealth;
        healthSlider.value = currentHealth;
        easeHealthSlider.maxValue = maxHealth;
        easeHealthSlider.value = currentHealth;



        float scaleFactor = maxHealth / initialMaxHealth;
        sliderTransform.sizeDelta = new Vector2(initialWidth *scaleFactor, sliderTransform.sizeDelta.y);
        easeSliderTransform.sizeDelta = new Vector2(initialWidth * scaleFactor, sliderTransform.sizeDelta.y);
    }



    private void Update()
    {
        if (currentHealth <= 0)
        {
            currentHealth = 0;
        }
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }



        if (healthSlider.value != currentHealth)
        {
            healthSlider.value = currentHealth;
        }



        if (healthSlider.value != easeHealthSlider.value && healthSlider.value > easeHealthSlider.value)
        {
            easeHealthSlider.value = healthSlider.value;
        }
        else if (healthSlider.value != easeHealthSlider.value && currentHealth == lastFrameHealth)
        {
            easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealth, lerpSpeed);
        }



        healthPercentage = Mathf.Clamp01(currentHealth / maxHealth);

        currentColorOfHealth = Color.Lerp(darkestHealthColor, originalHealthColor, healthPercentage);

        healthFillImage.color = currentColorOfHealth;
        healthFillImage.fillAmount = healthPercentage;



        lastFrameHealth = currentHealth;
    }
}
