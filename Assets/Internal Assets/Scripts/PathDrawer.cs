using System.Collections.Generic;
using UnityEngine;
using Mirror;

[ExecuteInEditMode]
public class PathDrawer : MonoBehaviour
{
    [Tooltip("Список точек пути")]
    public List<Vector3> pathPoints = new List<Vector3>();

    [Tooltip("Расстояние между точками при рисовании (м)")]
    public float pointSpacing = 1f;

    [Tooltip("Префаб автомобиля для этого пути")]
    public GameObject carPrefab;

    [Tooltip("Зациклить путь")]
    public bool isLooped = false;

    [HideInInspector]
    public Color pathColor;

    public int pathID;

    private static int nextID = 1;

    private void OnEnable()
    {
        if (pathColor == Color.clear)
            pathColor = Random.ColorHSV(0f, 1f, 0.7f, 1f, 0.8f, 1f);

        if (pathID == 0)
        {
            pathID = nextID++;
        }
    }

    // Метод для спавна техники на пути
    public GameObject SpawnVehicle()
    {
        if (carPrefab == null)
        {
            Debug.LogWarning($"На пути {name} нет carPrefab! Спавн отменён.");
            return null;
        }

        if (pathPoints == null || pathPoints.Count < 2)
        {
            Debug.LogWarning($"На пути {name} недостаточно точек для движения! Спавн отменён.");
            return null;
        }

        // Позиция и направление спавна
        Vector3 spawnPos = pathPoints[0];
        Vector3 direction = (pathPoints[1] - pathPoints[0]).normalized;
        Quaternion spawnRot = Quaternion.LookRotation(direction, Vector3.up);

        // Создаём объект на сервере
        GameObject car = Instantiate(carPrefab, spawnPos, spawnRot);
        NetworkServer.Spawn(car); // Спавним объект по сети
        Debug.Log($"Техника заспавнена на пути {name} в точке {spawnPos}");

        // Назначаем путь
        PathFollower pathFollower = car.GetComponent<PathFollower>();
        if (pathFollower != null)
        {
            pathFollower.SetPath(this); // Назначаем путь
            Debug.Log($"Path {name} назначен для {car.name}");
        }
        else
        {
            Debug.LogWarning($"У автомобиля {car.name} нет компонента PathFollower.");
        }

        return car;
    }

    private void OnDrawGizmos()
    {
        if (pathPoints == null || pathPoints.Count < 2) return;

        Gizmos.color = pathColor;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Gizmos.DrawLine(pathPoints[i], pathPoints[i + 1]);
            Gizmos.DrawSphere(pathPoints[i], 0.1f);
        }

        if (isLooped)
        {
            Gizmos.DrawLine(pathPoints[pathPoints.Count - 1], pathPoints[0]);
        }
    }
}
