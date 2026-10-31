using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookieCollectible : MonoBehaviour
{
    public AudioClip collectSound; // опционально: звук сбора

    private void OnTriggerEnter(Collider other)
    {
        // Проверяем, что коснулся игрок (у него должен быть тег "Player")
        if (other.CompareTag("Player"))
        {
            // Уведомляем GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CollectCookie();
            }

            // Воспроизводим звук
            if (collectSound != null)
                AudioSource.PlayClipAtPoint(collectSound, transform.position);

            // Уничтожаем печеньку
            Destroy(gameObject);
        }
    }
}