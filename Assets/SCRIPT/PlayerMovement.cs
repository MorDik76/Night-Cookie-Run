using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [Header("Настройки движения")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float crouchSpeed = 2f;
    
    [Header("Настройки прыжка и гравитации")]
    public float jumpHeight = 1.5f;
    public float gravity = -19.62f; 
    
    [Header("Настройки камеры")]
    public Transform cameraTransform; 
    public float mouseSensitivity = 200f;
    
    [Header("Настройки приседания")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;
    public float smoothSpeed = 10f;

    [Header("Настройки звука шагов")]
    public AudioClip[] footstepSounds;
    public float baseStepInterval = 0.5f;
    public float runStepMultiplier = 0.6f;
    public float crouchStepMultiplier = 1.5f;

    [Header("Здоровье")]
    public int health = 100;
    public int maxHealth = 100;
    public Slider healthSlider;
    public Text healthText;

    [Header("Боеприпасы")]
    public int arrows = 10;
    public int maxArrows = 30;
    public Text arrowsText;

    [Header("Оружие и предметы")]
    public bool hasBow = false;
    public bool hasFlashlight = false;

    [Header("Фонарик")]
    public Light flashlightLight;
    public KeyCode flashlightKey = KeyCode.F;

    [Header("UI смерти")]
    public GameObject deathPanel;
    public Text deathMessageText;

    [Header("Прицеливание (ПКМ)")]
    public bool enableAiming = true;
    public float aimFOV = 40f;
    public float normalFOV = 60f;
    public float aimSpeed = 8f;

    [Header("Прицел")]
    public GameObject crosshairImage;
    public Vector2 normalCrosshairSize = new Vector2(30, 30);
    public Vector2 aimCrosshairSize = new Vector2(15, 15);
    public Color normalCrosshairColor = Color.white;
    public Color aimCrosshairColor = Color.red;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching = false;
    private float xRotation = 0f;
    private float currentHeight;

    private AudioSource audioSource;
    private float stepTimer = 0f;
    private bool isDead = false;

    private Camera playerCamera;
    private float targetFOV;

    void Start()
    {
        Instance = this;

        controller = GetComponent<CharacterController>();
        
        if (!CompareTag("Player"))
            tag = "Player";
        
        if (GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        
        currentHeight = standingHeight;
        controller.height = currentHeight;
        controller.center = new Vector3(0, currentHeight / 2f, 0);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1f;
            audioSource.volume = 0.5f;
        }

        if (deathPanel != null)
            deathPanel.SetActive(false);

        UpdateArrowsUI();
        UpdateHealthUI();

        // Настройка камеры
        if (cameraTransform != null)
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
            if (playerCamera != null)
            {
                normalFOV = playerCamera.fieldOfView;
                targetFOV = normalFOV;
            }
        }

        // Прицел (виден только если есть лук)
        if (crosshairImage != null)
            crosshairImage.SetActive(false);

        // Фонарик изначально выключен
        if (flashlightLight != null)
            flashlightLight.enabled = false;

        Debug.Log("PlayerMovement инициализирован");
    }

    void Update()
    {
        if (isDead) return;

        HandleCameraRotation();
        HandleMovement();
        HandleFootsteps();
        if (enableAiming)
            HandleAiming();

        // Включение/выключение фонарика
        if (hasFlashlight && Input.GetKeyDown(flashlightKey))
        {
            if (flashlightLight != null)
                flashlightLight.enabled = !flashlightLight.enabled;
        }

        // Перезагрузка сцены по клавише R (для отладки)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void HandleCameraRotation()
    {
        if (cameraTransform == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        bool wantsToRun = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
        bool wantsToCrouch = Input.GetKey(KeyCode.LeftControl);

        if (wantsToCrouch && !isCrouching)
            isCrouching = true;
        else if (!wantsToCrouch && isCrouching)
            isCrouching = false;

        float targetHeight = isCrouching ? crouchingHeight : standingHeight;
        currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * smoothSpeed);

        controller.height = currentHeight;
        controller.center = new Vector3(0, currentHeight / 2f, 0);

        float currentSpeed = isCrouching ? crouchSpeed : (wantsToRun ? runSpeed : walkSpeed);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleFootsteps()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isMoving = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        if (isGrounded && isMoving && !isDead)
        {
            bool isRunning = Input.GetKey(KeyCode.LeftShift) && !isCrouching;
            float interval;
            if (isRunning)
                interval = baseStepInterval * runStepMultiplier;
            else if (isCrouching)
                interval = baseStepInterval * crouchStepMultiplier;
            else
                interval = baseStepInterval;

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                if (footstepSounds != null && footstepSounds.Length > 0)
                {
                    AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                    audioSource.PlayOneShot(clip);
                }
                stepTimer = interval * Random.Range(0.9f, 1.1f);
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void HandleAiming()
    {
        if (!hasBow) return;
        if (playerCamera == null) return;

        bool isAiming = Input.GetButton("Fire2");
        if (isAiming)
            targetFOV = aimFOV;
        else
            targetFOV = normalFOV;

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * aimSpeed);

        if (crosshairImage != null)
        {
            RectTransform rt = crosshairImage.GetComponent<RectTransform>();
            Image img = crosshairImage.GetComponent<Image>();
            if (rt != null)
            {
                Vector2 targetSize = isAiming ? aimCrosshairSize : normalCrosshairSize;
                rt.sizeDelta = Vector2.Lerp(rt.sizeDelta, targetSize, Time.deltaTime * aimSpeed);
            }
            if (img != null)
            {
                Color targetColor = isAiming ? aimCrosshairColor : normalCrosshairColor;
                img.color = Color.Lerp(img.color, targetColor, Time.deltaTime * aimSpeed);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        health -= damage;
        if (health < 0) health = 0;
        UpdateHealthUI();
        Debug.Log($"Игрок получил {damage} урона. Осталось: {health}");
        if (health <= 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        health += amount;
        if (health > maxHealth) health = maxHealth;
        UpdateHealthUI();
        Debug.Log($"Игрок вылечился на {amount}. Теперь здоровья: {health}");
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = health;
        }
        if (healthText != null)
            healthText.text = health.ToString();
    }

    void Die()
    {
        isDead = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            if (deathMessageText != null)
                deathMessageText.text = "Вас убил медведь... Нажмите «Restart»";
        }
        Debug.Log("Игрок умер!");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddArrows(int amount)
    {
        arrows = Mathf.Min(arrows + amount, maxArrows);
        UpdateArrowsUI();
        Debug.Log($"Добавлено {amount} стрел. Теперь: {arrows}");
    }

    public bool UseArrow()
    {
        if (arrows > 0)
        {
            arrows--;
            UpdateArrowsUI();
            Debug.Log($"Использована стрела. Осталось: {arrows}");
            return true;
        }
        else
        {
            Debug.Log("Нет стрел!");
            return false;
        }
    }

    private void UpdateArrowsUI()
    {
        if (arrowsText != null)
            arrowsText.text = $"Стрелы: {arrows}";
    }

    public void EquipBow()
    {
        hasBow = true;
        if (crosshairImage != null)
            crosshairImage.SetActive(true);
        Debug.Log("Лук подобран! Теперь можно стрелять (ПКМ для прицеливания, ЛКМ для выстрела).");
    }

    public void EquipFlashlight()
    {
        hasFlashlight = true;
        Debug.Log("Фонарик подобран! Нажмите F, чтобы включить/выключить.");
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}