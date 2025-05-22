using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using Input = UnityEngine.Input;



[RequireComponent(typeof(Controller2D))]



public class StaminaBar : MonoBehaviour
{
    public Slider staminaSlider;
    public RectTransform staminaSliderTransform;
    public Slider easeStaminaSlider;
    public RectTransform easeStaminaSliderTransform;

    float initialMaxStamina;
    float initialWidth;

    public float maxStamina = 100f;
    public float currentStamina = 100;
    [SerializeField] float lerpSpeed = 0.05f;



    [SerializeField] Image staminaFillImage;
    Color originalStaminaColor;
    Color darkestStaminaColor = new Color32(28,104,0,255);
    float staminaPercentage;
    Color currentColorOfStamina;



    float lastFrameStamina;



    Controller2D controller;



    float timeWaitingToStaminaRegen = 0f;
    float timeNeededToWaitUntilStaminaRegen = 2f;
    float staminaRegenSpeed = 30;

    bool startGainingStamina = false;



    private void Start()
    {
        currentStamina = maxStamina;
        lastFrameStamina = currentStamina;



        initialMaxStamina = maxStamina;
        initialWidth = staminaSliderTransform.sizeDelta.x;



        UpdateStaminaUI();



        originalStaminaColor = staminaFillImage.color;



        controller = GetComponent<Controller2D>();
    }



    private void UpdateStaminaUI()
    {
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = currentStamina;
        easeStaminaSlider.maxValue = maxStamina;
        easeStaminaSlider.value = currentStamina;



        float scaleFactor = maxStamina / initialMaxStamina;
        staminaSliderTransform.sizeDelta = new Vector2(initialWidth * scaleFactor, staminaSliderTransform.sizeDelta.y);
        easeStaminaSliderTransform.sizeDelta = new Vector2(initialWidth * scaleFactor, staminaSliderTransform.sizeDelta.y);
    }



    public void TakeStamina(float staminaTake)
    {
        currentStamina -= staminaTake;
    }



    private void Update()
    {
        if (currentStamina < lastFrameStamina)
        {
            startGainingStamina = false;
        }
        else if (controller.collisions.below)
        {
            startGainingStamina = true;
        }



        if (startGainingStamina == true)
        {
            if (timeWaitingToStaminaRegen < timeNeededToWaitUntilStaminaRegen)
            {
                timeWaitingToStaminaRegen += Time.deltaTime;
            }
            else
            {
                timeWaitingToStaminaRegen = timeNeededToWaitUntilStaminaRegen;
            }
        }
        else if (startGainingStamina == false)
        {
            timeWaitingToStaminaRegen = 0;
        }

        if (timeWaitingToStaminaRegen >= timeNeededToWaitUntilStaminaRegen)
        {
            currentStamina += staminaRegenSpeed * Time.deltaTime;
        }



        if (currentStamina <= 0)
        {
            currentStamina = 0;
        }
        if (currentStamina >= maxStamina)
        {
            currentStamina = maxStamina;
        }




        if (staminaSlider.value != currentStamina)
        {
            staminaSlider.value = currentStamina;
        }



        if (staminaSlider.value != easeStaminaSlider.value && staminaSlider.value > easeStaminaSlider.value)
        {
            easeStaminaSlider.value = staminaSlider.value;
        }
        else if (staminaSlider.value != easeStaminaSlider.value && currentStamina == lastFrameStamina)
        {
            easeStaminaSlider.value = Mathf.Lerp(easeStaminaSlider.value, currentStamina, lerpSpeed);
        }



        staminaPercentage = Mathf.Clamp01(currentStamina / maxStamina);

        currentColorOfStamina = Color.Lerp(darkestStaminaColor, originalStaminaColor, staminaPercentage);

        staminaFillImage.color = currentColorOfStamina;
        staminaFillImage.fillAmount = staminaPercentage;



        lastFrameStamina = currentStamina;
    }
}
