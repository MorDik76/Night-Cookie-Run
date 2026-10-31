using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearHealth : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;
    public AudioClip deathSound;
    public GameObject deathEffect;

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log($"BearHealth: Медведь {gameObject.name} инициализирован с {maxHealth} HP");
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0)
        {
            Debug.Log($"BearHealth: Медведь уже мёртв!");
            return;
        }

        currentHealth -= damage;
        Debug.Log($"BearHealth: Медведь получил {damage} урона. Осталось: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log($"BearHealth: Медведь {gameObject.name} УМЕР!");
        if (deathSound != null)
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        
        BearAI ai = GetComponent<BearAI>();
        if (ai != null) ai.Die();
        else Destroy(gameObject);
    }
}