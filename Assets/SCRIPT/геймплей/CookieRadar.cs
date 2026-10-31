using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;      // для обычного UI Text
using TMPro;               // раскомментируйте, если используете TextMeshPro

public class CookieRadar : MonoBehaviour
{
    [Header("Настройки радара")]
    public float pingInterval = 60f;      // интервал между пингами (секунд)
    public float displayDuration = 3f;    // сколько секунд показывать подсказку
    public float updateInterval = 0.2f;   // как часто обновлять расстояние во время отображения
    public KeyCode manualPingKey = KeyCode.R; // клавиша для ручного пинга

    [Header("UI")]
    public Text distanceText;              // сюда перетащите UI Text (обычный)
    // public TextMeshProUGUI distanceText; // если используете TMP, раскомментируйте и замените выше

    private GameObject[] allCookies;       // все объекты с тегом "Cookie"
    private Coroutine activePingCoroutine;

    void Start()
    {
        if (distanceText != null)
            distanceText.gameObject.SetActive(false); // скрываем подсказку в начале

        // Запускаем периодический пинг
        InvokeRepeating(nameof(PingNearestCookie), pingInterval, pingInterval);
    }

    void Update()
    {
        if (Input.GetKeyDown(manualPingKey))
        {
            PingNearestCookie();
        }
    }

    void PingNearestCookie()
    {
        // Находим все печеньки (активные)
        allCookies = GameObject.FindGameObjectsWithTag("Cookie");
        
        // Если печенек нет – выводим сообщение и выходим
        if (allCookies.Length == 0)
        {
            ShowText("Нет печенек на уровне!");
            return;
        }

        // Находим ближайшую
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        Vector3 playerPos = transform.position;

        foreach (GameObject cookie in allCookies)
        {
            if (!cookie.activeInHierarchy) continue; // пропускаем неактивные
            float dist = Vector3.Distance(playerPos, cookie.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = cookie;
            }
        }

        if (nearest == null)
        {
            ShowText("Нет активных печенек!");
            return;
        }

        // Запускаем отображение с обновлением расстояния до ближайшей
        if (activePingCoroutine != null)
            StopCoroutine(activePingCoroutine);
        activePingCoroutine = StartCoroutine(UpdateDistanceDisplay(nearest));
    }

    IEnumerator UpdateDistanceDisplay(GameObject targetCookie)
    {
        if (distanceText == null) yield break;

        distanceText.gameObject.SetActive(true);
        float elapsed = 0f;
        float nextUpdate = 0f;

        while (elapsed < displayDuration)
        {
            // Проверяем, существует ли ещё печенька и активна ли
            if (targetCookie == null || !targetCookie.activeInHierarchy)
            {
                // Если цель исчезла (собрали), показываем сообщение и завершаем
                distanceText.text = "Печенька собрана!";
                yield break;
            }

            // Обновляем расстояние
            float dist = Vector3.Distance(transform.position, targetCookie.transform.position);
            distanceText.text = $"Печенька: {dist:F1} метров от вас";

            // Ждём следующий интервал обновления
            yield return new WaitForSeconds(updateInterval);
            elapsed += updateInterval;
        }

        // Скрываем текст после завершения
        distanceText.gameObject.SetActive(false);
        activePingCoroutine = null;
    }

    void ShowText(string message)
    {
        if (distanceText == null) return;
        
        if (activePingCoroutine != null)
            StopCoroutine(activePingCoroutine);
        
        distanceText.gameObject.SetActive(true);
        distanceText.text = message;
        StartCoroutine(HideTextAfterDelay(2f));
    }

    IEnumerator HideTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (distanceText != null)
            distanceText.gameObject.SetActive(false);
        activePingCoroutine = null;
    }
}