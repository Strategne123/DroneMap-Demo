using UnityEngine;

public class PathFollower : MonoBehaviour
{
    [Header("Настройки пути")]
    public float steeringSensitivity = 1.5f;
    public float reachThreshold = 0.5f;
    public float maxSpeed = 50f;

    private int currentPointIndex = 0;
    private PathDrawer pathDrawer;
    private CarController carController;

    void Start()
    {
        carController = GetComponent<CarController>();
        if (carController == null)
        {
            Debug.LogError($"CarController не найден на {name}. Отключаю PathFollower.");
            enabled = false;
            return;
        }

        // Не отключаем компонент, даже если pathDrawer == null
    }

    public void SetPath(PathDrawer path)
    {
        pathDrawer = path;
        currentPointIndex = 0;
        Debug.Log($"Путь {path.name} назначен для {name}.");
    }

    void FixedUpdate()
    {
        if (pathDrawer == null || pathDrawer.pathPoints.Count == 0)
        {
            carController.SetInput(0f, 0f);
            return;
        }

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        if (currentPointIndex >= pathDrawer.pathPoints.Count)
        {
            if (pathDrawer.isLooped)
            {
                currentPointIndex = 0;
                Debug.Log($"Машина {name} зациклилась на пути.");
            }
            else
            {
                Debug.Log($"Машина {name} завершила путь.");
                carController.SetInput(0f, 0f);
                return;
            }
        }

        Vector3 targetPoint = pathDrawer.pathPoints[currentPointIndex];
        Vector3 frontOfCar = transform.position + transform.forward * carController.frontOffset;
        float distanceToTarget = Vector3.Distance(frontOfCar, targetPoint);

        if (distanceToTarget < reachThreshold)
        {
            currentPointIndex++;
            if (currentPointIndex >= pathDrawer.pathPoints.Count)
            {
                if (pathDrawer.isLooped)
                {
                    currentPointIndex = 0;
                    Debug.Log($"Машина {name} зациклилась на пути.");
                }
                else
                {
                    Debug.Log($"Машина {name} завершила путь.");
                    carController.SetInput(0f, 0f);
                    return;
                }
            }
        }

        Vector3 directionToTarget = (targetPoint - frontOfCar).normalized;
        float angleToTarget = Vector3.SignedAngle(transform.forward, directionToTarget, Vector3.up);

        float steering = Mathf.Clamp(angleToTarget / 45f, -1f, 1f) * steeringSensitivity;
        carController.SetInput(1f, steering);
    }
}
