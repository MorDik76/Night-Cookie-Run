using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("UI паузы")]
    public GameObject pausePanel;
    public Button continueButton;
    public Button exitButton;

    [Header("Настройки")]
    public KeyCode pauseKey = KeyCode.Escape;
    public bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (pausePanel == null)
            Debug.LogError("Pause Panel не назначен!");

        if (continueButton != null)
            continueButton.onClick.AddListener(ContinueGame);
        else
            Debug.LogWarning("Continue Button не назначен!");

        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);
        else
            Debug.LogWarning("Exit Button не назначен!");

        if (pausePanel != null)
            pausePanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (GameManager.Instance != null && GameManager.Instance.gameCompleted)
                return;

            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ContinueGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ContinueGame);
        if (exitButton != null)
            exitButton.onClick.RemoveListener(ExitGame);
    }
}