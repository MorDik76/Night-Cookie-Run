using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Arrow : MonoBehaviour
{
    public int damage = 30;
    public GameObject owner;
    public AudioClip hitSound;
    private AudioSource audioSource;
    private bool hasHit = false;

    void Start()
    {
        Debug.Log("Arrow: Start() вызван");

        // Принудительная настройка Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.mass = 0.1f;
            rb.drag = 0.05f;
            Debug.Log("Arrow: Rigidbody настроен автоматически");
        }
        else
        {
            Debug.LogError("Arrow: Rigidbody НЕ НАЙДЕН!");
        }

        // Настройка AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
    }

    void Update()
    {
        // Для отладки: проверка скорости
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && rb.velocity.magnitude > 0.1f)
        {
            // Раз в 60 кадров выводим скорость (не засоряем консоль)
            if (Time.frameCount % 60 == 0)
                Debug.Log($"Arrow: скорость = {rb.velocity.magnitude}");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        hasHit = true;

        Debug.Log($"Arrow: Попали в {collision.gameObject.name}, тег: {collision.gameObject.tag}");

        // Не наносим урон владельцу
        if (owner != null && collision.gameObject == owner)
        {
            Debug.Log("Arrow: Попали в владельца (игрока) — пропускаем");
            Destroy(gameObject);
            return;
        }

        // Ищем BearHealth
        BearHealth bear = collision.gameObject.GetComponent<BearHealth>();
        if (bear != null)
        {
            bear.TakeDamage(damage);
            Debug.Log($"Arrow: Урон {damage} по медведю!");
            if (hitSound != null)
                audioSource.PlayOneShot(hitSound);
        }
        else
        {
            Debug.Log($"Arrow: BearHealth НЕ НАЙДЕН на {collision.gameObject.name}");
        }

        // Уничтожаем стрелу через 0.1 секунды
        Destroy(gameObject, 0.1f);
    }
}