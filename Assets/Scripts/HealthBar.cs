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
    }

    public float TakeDamage(int damage)
    {
        return health -= damage;
    }
}
