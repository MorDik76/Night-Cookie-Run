using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // <-- Обязательно подключите эту библиотеку для работы с TMP
public class StoryManager : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI storyTextField;

    [Header("Story Data")]
    public string[] storyParts;

    [Header("Typing Settings")]
    public float timePerChar = 0.05f;  // задержка между символами

    private int currentPartIndex = 0;
    private bool isTyping = false;
    private bool cancelTyping = false;

    void Start()
    {
        if (storyParts.Length > 0)
        {
            StartCoroutine(TypeText(storyParts[currentPartIndex]));
        }
        else
        {
            storyTextField.text = "Нажмите любую клавишу, чтобы начать...";
        }
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            if (isTyping)
            {
                // Если текст сейчас печатается – прерываем анимацию и показываем весь текст сразу
                cancelTyping = true;
            }
            else
            {
                // Если печать завершена – переходим к следующей части
                NextStoryPart();
            }
        }
    }

    void NextStoryPart()
    {
        currentPartIndex++;
        if (currentPartIndex < storyParts.Length)
        {
            StartCoroutine(TypeText(storyParts[currentPartIndex]));
        }
        else
        {
            // История закончена – убираем надпись "Конец истории" и отключаем скрипт (необязательно)
            storyTextField.text = "";
            this.enabled = false; // скрипт больше не будет обрабатывать нажатия
        }
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        storyTextField.text = textToType;
        storyTextField.maxVisibleCharacters = 0;
        cancelTyping = false;

        int totalChars = textToType.Length;

        for (int i = 0; i <= totalChars; i++)
        {
            if (cancelTyping)
            {
                // Пропуск анимации – показываем весь текст сразу
                storyTextField.maxVisibleCharacters = totalChars;
                break;
            }
            storyTextField.maxVisibleCharacters = i;
            yield return new WaitForSeconds(timePerChar);
        }

        isTyping = false;
    }
}