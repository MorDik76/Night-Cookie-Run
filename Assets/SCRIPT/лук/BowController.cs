using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowController : MonoBehaviour
{
    [Header("Настройки стрельбы")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowSpeed = 30f;
    public int arrowDamage = 30;
    public float fireRate = 0.8f;

    [Header("Звук")]
    public AudioClip shootSound;
    public AudioSource audioSource;

    private float nextFireTime = 0f;
    private Camera playerCamera;
    private PlayerMovement player;

    void Start()
    {
        player = GetComponent<PlayerMovement>();
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null && shootSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Если нет лука (система подбора) — не стреляем
        if (player != null && !player.hasBow) return;
        if (Time.time < nextFireTime) return;

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    void Shoot()
    {
        Debug.Log("=== ВЫСТРЕЛ ===");

        // Проверка игрока
        if (player == null)
        {
            Debug.LogError("BowController: PlayerMovement не найден!");
            return;
        }

        // Проверка стрел
        if (!player.UseArrow())
        {
            Debug.Log("Нет стрел!");
            return;
        }

        // Проверка префаба
        if (arrowPrefab == null)
        {
            Debug.LogError("BowController: arrowPrefab не назначен!");
            return;
        }

        // Проверка firePoint
        if (firePoint == null)
        {
            Debug.LogError("BowController: firePoint не назначен!");
            return;
        }

        // Проверка камеры
        if (playerCamera == null)
        {
            Debug.LogError("BowController: камера не найдена!");
            return;
        }

        // Направление выстрела
        Vector3 shootDirection = playerCamera.transform.forward;
        Debug.Log($"Направление: {shootDirection}");

        // Создаём стрелу
        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
        Debug.Log($"Стрела создана в {firePoint.position}");

        // Настраиваем физику
        Rigidbody rb = arrow.GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("У стрелы НЕТ Rigidbody!");
            return;
        }

        // Принудительно настраиваем физику
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.velocity = shootDirection * arrowSpeed;
        
        Debug.Log($"Скорость: {rb.velocity} (arrowSpeed = {arrowSpeed})");
        Debug.Log($"UseGravity = {rb.useGravity}, isKinematic = {rb.isKinematic}");

        // Настраиваем скрипт стрелы
        Arrow arrowScript = arrow.GetComponent<Arrow>();
        if (arrowScript != null)
        {
            arrowScript.damage = arrowDamage;
            arrowScript.owner = gameObject;
        }

        // Звук
        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound, 0.8f);

        // Автоуничтожение через 5 секунд
        Destroy(arrow, 5f);
    }
}