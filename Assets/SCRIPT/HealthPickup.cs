using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int healAmount = 25;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("HealthPickup: OnTriggerEnter с " + other.gameObject.name);

        if (other.CompareTag("Player"))
        {
            PlayerMovement player = other.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.Heal(healAmount);
                if (pickupSound != null)
                    AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                Destroy(gameObject);
                Debug.Log("Гриб собран, здоровье восстановлено на " + healAmount);
            }
            else
            {
                Debug.LogError("На игроке нет компонента PlayerMovement!");
            }
        }
        else
        {
            Debug.Log("Объект " + other.gameObject.name + " не имеет тега Player");
        }
    }
}