
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class SkyboxManager : MonoBehaviour
{
    [Header("Настройки Skybox")]
    public Material skyboxBlendMaterial; // материал с шейдером SkyboxBlend
    public float transitionDuration = 3f; // длительность плавного перехода (сек)

    private Coroutine currentTransition;

    private void Start()
    {
        if (skyboxBlendMaterial == null)
        {
            Debug.LogError("Skybox blend material не назначен!");
            return;
        }

        // Устанавливаем Skybox
        RenderSettings.skybox = skyboxBlendMaterial;

        // Начинаем с ДНЯ (Blend12 = 0, Blend23 = 0)
        skyboxBlendMaterial.SetFloat("_Blend12", 0f);
        skyboxBlendMaterial.SetFloat("_Blend23", 0f);
    }

    // Переход на ВЕЧЕР (Sky1 -> Sky2)
    public void TransitionToEvening()
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(BlendSkybox(1f, 0f, transitionDuration));
        Debug.Log("Начинается переход на ВЕЧЕР");
    }

    // Переход на НОЧЬ (Sky2 -> Sky3)
    public void TransitionToNight()
    {
        if (currentTransition != null)
            StopCoroutine(currentTransition);
        currentTransition = StartCoroutine(BlendSkybox(1f, 1f, transitionDuration));
        Debug.Log("Начинается переход на НОЧЬ");
    }

    // Плавное изменение параметров Blend
    private IEnumerator BlendSkybox(float targetBlend12, float targetBlend23, float duration)
    {
        float startBlend12 = skyboxBlendMaterial.GetFloat("_Blend12");
        float startBlend23 = skyboxBlendMaterial.GetFloat("_Blend23");

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float smoothT = t * t * (3f - 2f * t); // SmoothStep

            float currentBlend12 = Mathf.Lerp(startBlend12, targetBlend12, smoothT);
            float currentBlend23 = Mathf.Lerp(startBlend23, targetBlend23, smoothT);

            skyboxBlendMaterial.SetFloat("_Blend12", currentBlend12);
            skyboxBlendMaterial.SetFloat("_Blend23", currentBlend23);

            yield return null;
        }

        // Фиксируем конечные значения
        skyboxBlendMaterial.SetFloat("_Blend12", targetBlend12);
        skyboxBlendMaterial.SetFloat("_Blend23", targetBlend23);
        currentTransition = null;
    }
}