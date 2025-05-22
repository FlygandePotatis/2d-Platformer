using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public float enemeyHealth = 100f;
    float currentHealth;


    private void Start()
    {
        currentHealth = enemeyHealth;
    }
}
