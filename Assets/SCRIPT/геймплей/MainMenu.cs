using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Метод для запуска игры
    public void PlayGame()
    {
        // Загружает следующую сцену в очереди Build Settings
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Метод для выхода из игры
    public void QuitGame()
    {
        Debug.Log("Игра закрыта!"); // Сработает в редакторе Unity
        Application.Quit();
    }
}