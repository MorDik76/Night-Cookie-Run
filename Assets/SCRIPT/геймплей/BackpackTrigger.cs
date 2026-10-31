using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BackpackTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.WinGame();
            }
        }
    }
}