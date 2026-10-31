using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearHitZone : MonoBehaviour
{
    private BearHealth bearHealth;

    void Start()
    {
        // Находим компонент BearHealth на родительском объекте (медведь)
        bearHealth = GetComponentInParent<BearHealth>();
        if (bearHealth == null)
            Debug.LogError("BearHitZone: не найден BearHealth на родителе!");
    }

    void OnCollisionEnter(Collision collision)
    {
        // Проверяем, что столкновение произошло со стрелой
        Arrow arrow = collision.gameObject.GetComponent<Arrow>();
        if (arrow != null)
        {
            bearHealth.TakeDamage(arrow.damage);
            // Звук попадания можно воспроизвести здесь или в стреле
            // Уничтожаем стрелу после попадания
            Destroy(collision.gameObject);
        }
    }
}