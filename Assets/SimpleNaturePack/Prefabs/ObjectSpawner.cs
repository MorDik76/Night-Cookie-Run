using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject objectToSpawn; // Сюда перетащите ваш Prefab
    public int numberOfObjects = 200; // Сколько объектов нужно расставить

    // Границы зоны, в которой будут появляться объекты
    public Vector2 spawnAreaSize = new Vector2(50f, 50f); 

    void Start()
    {
        SpawnObjects();
    }

    void SpawnObjects()
    {
        for (int i = 0; i < numberOfObjects; i++)
        {
            // Выбираем случайные координаты X и Z в заданном радиусе
            float randomX = Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2);
            float randomZ = Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2);

            // Создаем случайную позицию
            Vector3 spawnPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

            // Создаем объект на локации
            GameObject newObject = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

            // Случайный поворот от 0 до 360 градусов, чтобы объекты не выглядели одинаково
            float randomYRotation = Random.Range(0f, 360f);
            newObject.transform.Rotate(0, randomYRotation, 0);
        }
    }

    // Показывает зону спавна в окне Scene для удобства
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnAreaSize.x, 1f, spawnAreaSize.y));
    }
}