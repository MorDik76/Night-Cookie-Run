using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PickupType { Arrows, Health, Bow, Flashlight }
public class PickupItem : MonoBehaviour
{
    public PickupType type = PickupType.Arrows;
    public int amount = 5;          // количество стрел или хилки
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player == null) return;

        switch (type)
        {
            case PickupType.Arrows:
                player.AddArrows(amount);
                break;
            case PickupType.Health:
                player.Heal(amount);
                break;
            case PickupType.Bow:
                player.EquipBow();
                break;
            case PickupType.Flashlight:
                player.EquipFlashlight();
                break;
        }

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        Destroy(gameObject);
    }
}