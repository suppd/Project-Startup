using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{
    public Image healthBar;
    public float health;
    public float maxHealth;

    //Damage over time
    float timer = 0;
    public bool takenDamage;
    public float maxTime = 2;
    void Start()
    {
        health = 100;
        maxHealth = 100;
    }

    void Update()
    {
        healthBar.fillAmount = health / maxHealth;

        if (health <= 0)
        {
            SceneManager.LoadScene("Level layout");
        }
        TakeDamageOverTime(1);
    }

    public void TakeDamageOverTime(int damage)
    {
        if (health > 0)
        {
            if (!takenDamage)
            {
                timer += Time.deltaTime;    
                if (timer > maxTime)
                {
                    takenDamage = true;
                }
            }
            if (takenDamage)
            {
                health -= damage;
                timer = 0;
                takenDamage = false;
            }

        }
    }

    public float TakeDamage(int damage)
    {
        return health -= damage;
    }
}
