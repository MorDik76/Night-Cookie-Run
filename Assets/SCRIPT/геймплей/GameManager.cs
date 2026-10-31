using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Настройки печенек")]
    public int totalCookies = 20;
    public int eveningThreshold = 10;
    private int collectedCookies = 0;

    [Header("Состояние игры")]
    public bool isNight = false;
    public bool isEvening = false;
    public bool gameCompleted = false;

    [Header("UI")]
    public Text cookiesText;
    public GameObject nightMessagePanel;
    public GameObject eveningMessagePanel;

    [Header("Освещение")]
    public Light sunLight;
    public Color dayColor = Color.white;
    public Color eveningColor = new Color(0.8f, 0.5f, 0.2f);
    public Color nightColor = new Color(0.1f, 0.1f, 0.2f);
    public float dayIntensity = 1f;
    public float eveningIntensity = 0.6f;
    public float nightIntensity = 0.05f;

    [Header("Skybox Manager")]
    public SkyboxManager skyboxManager;

    [Header("Фонарь")]
    public GameObject flashlightPrefab;
    public Transform flashlightHolder;
    private GameObject currentFlashlight;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateUI();
        if (nightMessagePanel != null) nightMessagePanel.SetActive(false);
        if (eveningMessagePanel != null) eveningMessagePanel.SetActive(false);

        SetDaySettings();
    }

    public void CollectCookie()
    {
        if (gameCompleted || isNight) return;

        collectedCookies++;
        UpdateUI();

        if (!isEvening && collectedCookies >= eveningThreshold)
        {
            TransitionToEvening();
        }

        if (collectedCookies >= totalCookies && !isNight)
        {
            StartNight();
        }
    }

    private void TransitionToEvening()
    {
        isEvening = true;
        Debug.Log("Наступил ВЕЧЕР! Собрано " + collectedCookies + " печенек.");

        if (sunLight != null)
        {
            sunLight.color = eveningColor;
            sunLight.intensity = eveningIntensity;
        }

        if (skyboxManager != null)
        {
            skyboxManager.TransitionToEvening();
        }

        if (eveningMessagePanel != null)
            eveningMessagePanel.SetActive(true);
    }

    private void StartNight()
    {
        isNight = true;
        Debug.Log("Наступила НОЧЬ! Бегите к рюкзаку!");

        if (sunLight != null)
        {
            sunLight.color = nightColor;
            sunLight.intensity = nightIntensity;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.03f, 0.03f, 0.05f);
        RenderSettings.reflectionIntensity = 0f;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 3f;
        RenderSettings.fogEndDistance = 15f;
        RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.1f);

        if (skyboxManager != null)
        {
            skyboxManager.TransitionToNight();
        }

        GameObject[] cookies = GameObject.FindGameObjectsWithTag("Cookie");
        foreach (GameObject cookie in cookies) Destroy(cookie);

        SpawnFlashlight();

        if (eveningMessagePanel != null) eveningMessagePanel.SetActive(false);
        if (nightMessagePanel != null) nightMessagePanel.SetActive(true);
    }

    private void SpawnFlashlight()
    {
        if (flashlightPrefab == null)
        {
            Debug.LogWarning("Flashlight prefab не назначен!");
            return;
        }

        if (flashlightHolder == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                flashlightHolder = cam.transform.Find("FlashlightHolder");
            if (flashlightHolder == null)
            {
                Debug.LogError("FlashlightHolder не найден!");
                return;
            }
        }

        if (currentFlashlight != null)
            Destroy(currentFlashlight);

        currentFlashlight = Instantiate(flashlightPrefab, flashlightHolder);
        currentFlashlight.transform.localPosition = Vector3.zero;
        currentFlashlight.transform.localRotation = Quaternion.identity;

        Light flashlightLight = currentFlashlight.GetComponentInChildren<Light>();
        if (flashlightLight != null)
            flashlightLight.enabled = true;

        Debug.Log("Фонарь появился!");
    }

    public void WinGame()
    {
        if (gameCompleted) return;

        if (isNight)
        {
            gameCompleted = true;
            Debug.Log("ПОБЕДА!");

            if (PauseManager.Instance != null && PauseManager.Instance.isPaused)
            {
                PauseManager.Instance.ContinueGame();
            }

            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Debug.Log("Сначала соберите все печеньки!");
        }
    }

    private void UpdateUI()
    {
        if (cookiesText != null)
            cookiesText.text = $"Печеньки: {collectedCookies} / {totalCookies}";
    }

    private void SetDaySettings()
    {
        if (sunLight != null)
        {
            sunLight.color = dayColor;
            sunLight.intensity = dayIntensity;
        }

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        RenderSettings.ambientLight = Color.gray;
        RenderSettings.reflectionIntensity = 1f;
        RenderSettings.fog = false;
    }
}