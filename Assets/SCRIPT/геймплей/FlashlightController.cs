using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FlashlightController : MonoBehaviour
{
    private GameObject flashlightObject;

    void Update()
    {
        // Ищем фонарь в сцене (если он уже создан GameManager)
        if (flashlightObject == null)
            flashlightObject = GameObject.FindGameObjectWithTag("Flashlight");

        if (flashlightObject != null && Input.GetKeyDown(KeyCode.F))
        {
            Light lightComp = flashlightObject.GetComponentInChildren<Light>();
            if (lightComp != null)
                lightComp.enabled = !lightComp.enabled;
        }
    }
}