using UnityEngine;
using UnityEngine.UI;



public class EnemyHealthScript : MonoBehaviour
{
    [SerializeField] Slider enemyHealthSlider;
    [SerializeField] RectTransform enemySliderTransform;
    [SerializeField] Slider enemnyEaseHealthSlider;
    [SerializeField] RectTransform enemyEaseSliderTransform;

    float enemyInitialMaxHealth;
    float enemyInitialWidth;

    [SerializeField] float enemyMaxHealth = 100f;
    public float enemyCurrentHealth = 100;
    [SerializeField] float lerpSpeed = 0.008f;



    [SerializeField] Image healthFillImage;
    Color originalHealthColor;
    Color darkestHealthColor = new Color32(133, 0, 0, 255);
    float healthPercentage;
    Color currentColorOfHealth;



    float lastFrameHealth;



    bool enemyDying = false;



    [SerializeField] GameObject enemyHealthBarCanvas;



    [SerializeField] float timeToDissapearEnemyHealthBar = 1f;
    float timeThatHasGoneBy = 0f;



    private void Start()
    {
        enemyCurrentHealth = enemyMaxHealth;
        lastFrameHealth = enemyCurrentHealth;



        enemyInitialMaxHealth = enemyMaxHealth;
        enemyInitialWidth = enemySliderTransform.sizeDelta.x;



        UpdateHealthUI();



        originalHealthColor = healthFillImage.color;



        timeThatHasGoneBy = timeToDissapearEnemyHealthBar;
    }



    void UpdateHealthUI()
    {
        enemyHealthSlider.maxValue = enemyMaxHealth;
        enemyHealthSlider.value = enemyCurrentHealth;
        enemnyEaseHealthSlider.maxValue = enemyMaxHealth;
        enemnyEaseHealthSlider.value = enemyCurrentHealth;



        float scaleFactor = enemyMaxHealth / enemyInitialMaxHealth;
        enemySliderTransform.sizeDelta = new Vector2(enemyInitialWidth * scaleFactor, enemySliderTransform.sizeDelta.y);
        enemyEaseSliderTransform.sizeDelta = new Vector2(enemyInitialWidth * scaleFactor, enemySliderTransform.sizeDelta.y);
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            enemyCurrentHealth += 30;
        }



        if (enemyCurrentHealth <= 0)
        {
            enemyCurrentHealth = 0;
        }
        if (enemyCurrentHealth >= enemyMaxHealth)
        {
            enemyCurrentHealth = enemyMaxHealth;
        }



        if (enemyCurrentHealth == enemyMaxHealth)
        {
            timeThatHasGoneBy += Time.deltaTime;
        }
        else
        {
            timeThatHasGoneBy = 0;
        }

        if (timeThatHasGoneBy >= timeToDissapearEnemyHealthBar)
        {
            enemyHealthBarCanvas.SetActive(false);
        }
        else
        {
            enemyHealthBarCanvas.SetActive(true);
        }



        if (enemyHealthSlider.value != enemyCurrentHealth)
        {
            enemyHealthSlider.value = enemyCurrentHealth;
        }



        if (enemyHealthSlider.value != enemnyEaseHealthSlider.value && enemyHealthSlider.value > enemnyEaseHealthSlider.value)
        {
            enemnyEaseHealthSlider.value = enemyHealthSlider.value;
        }
        else if (enemyHealthSlider.value != enemnyEaseHealthSlider.value && enemyCurrentHealth == lastFrameHealth)
        {
            enemnyEaseHealthSlider.value = Mathf.Lerp(enemnyEaseHealthSlider.value, enemyCurrentHealth, lerpSpeed);
        }



        healthPercentage = Mathf.Clamp01(enemyCurrentHealth / enemyMaxHealth);

        currentColorOfHealth = Color.Lerp(darkestHealthColor, originalHealthColor, healthPercentage);

        healthFillImage.color = currentColorOfHealth;
        healthFillImage.fillAmount = healthPercentage;



        lastFrameHealth = enemyCurrentHealth;
    }



    public void enemyTakeDamageByPlayer(float damage)
    {
        if (enemyCurrentHealth > 0)
        {
            enemyCurrentHealth -= damage;



            //animation hurt - kanske flytta på när animationen görs beroende på hitstop



            if (enemyCurrentHealth <= 0 && !enemyDying)
            {
                enemyDie();
            }
        }
    }



    void enemyDie()
    {
        enemyDying = true;



        gameObject.layer = LayerMask.NameToLayer("DeadEnemies");



        //die animation



        //disable enemy (fix) begin here
        //GetComponent<Collider2D>().enabled = false;



        //this.enabled = false;
        //end here
    }
}
