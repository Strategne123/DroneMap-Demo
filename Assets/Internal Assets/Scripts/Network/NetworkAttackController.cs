using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NetworkAttackController : NetworkBehaviour
{
    private DroneNetworkSettings _droneNetworkSettings;

    [SerializeField] private GameObject[] cars;
    [SerializeField] private GameObject[] tanks;
    [SerializeField] private GameObject[] armoredVehicles;

    [SerializeField] private float carRatio = 1f;
    [SerializeField] private float tankRatio = 0f;
    [SerializeField] private float armoredVehicleRatio = 0f;

    private List<GameObject> enemies = new List<GameObject>();
    private List<REB> rebList = new List<REB>();
    public bool newVersion;
    private List<CarAIController> carAI = new List<CarAIController>();
    private List<NavMeshAgent> agents = new List<NavMeshAgent>();

    public override void OnStartServer()
    {
        StartCoroutine(Init());
    }

    private IEnumerator Init()
    {
        // Ожидание, пока DroneNetworkSettings не будет инициализирован
        while (_droneNetworkSettings == null)
        {
            _droneNetworkSettings = FindServerDroneNetworkSettings();
            yield return new WaitForEndOfFrame();
        }

        while (!_droneNetworkSettings.isInitialized)
        {
            yield return new WaitForEndOfFrame();
        }

        // Получаем параметры от _droneNetworkSettings
        int carsCount = _droneNetworkSettings.GetCountCars();
        SpawnCars(carsCount);

        int reb = _droneNetworkSettings.GetCarREB();
        float speed = _droneNetworkSettings.GetCarSpeed();

        SetREB((eREB)reb);
        SetSpeed(speed);
        SetValuesToNetworkCars(reb, speed);
    }

    private DroneNetworkSettings FindServerDroneNetworkSettings()
    {
        DroneNetworkSettings[] allSettings = FindObjectsOfType<DroneNetworkSettings>();
        foreach (var settings in allSettings)
        {
            if (settings.isOwned)
            {
                return settings;
            }
        }
        return null;
    }

    // Метод для спавна машин
    public void SpawnCars(int count)
    {
        // Очистка старых объектов
        while (enemies.Count > 0)
        {
            var enemy = enemies[0];
            enemies.RemoveAt(0);
            rebList.RemoveAt(0);
            if (newVersion)
            {
                carAI.RemoveAt(0);
            }
            else
            {
                agents.RemoveAt(0);
            }
            Destroy(enemy);
        }
        GroundPointsHandler.ClearStartPos();

        float totalRatio = carRatio + tankRatio + armoredVehicleRatio;

        for (int i = 0; i < count; i++)
        {
            GameObject prefabToSpawn = null;
            float randomValue = Random.Range(0f, totalRatio);

            if (randomValue <= carRatio)
            {
                prefabToSpawn = cars[Random.Range(0, cars.Length)];
            }
            else if (randomValue <= carRatio + tankRatio)
            {
                prefabToSpawn = tanks[Random.Range(0, tanks.Length)];
            }
            else
            {
                prefabToSpawn = armoredVehicles[Random.Range(0, armoredVehicles.Length)];
            }

            var enemy = Instantiate(prefabToSpawn, transform);
            if (NetworkServer.active)
                NetworkServer.Spawn(enemy);

            rebList.Add(enemy.GetComponentInChildren<REB>());
            if (newVersion)
            {
                carAI.Add(enemy.GetComponent<CarAIController>());
            }
            else
            {
                agents.Add(enemy.GetComponent<NavMeshAgent>());
            }
            enemies.Add(enemy);
        }
    }

    // Устанавливаем соотношения типов машин (например, танков и бронемашин)
    public void SetRatios(float carRatio, float tankRatio, float armoredVehicleRatio)
    {
        this.carRatio = carRatio;
        this.tankRatio = tankRatio;
        this.armoredVehicleRatio = armoredVehicleRatio;

        int currentCount = enemies.Count;
        SpawnCars(currentCount);
    }

    // Устанавливаем значения для сетевых машин
    public void SetValuesToNetworkCars(int reb, float speed)
    {
        foreach (var enemy in enemies)
        {
            var networkCar = enemy.GetComponent<NetworkCar>();
            networkCar.rebValue = reb;
            networkCar.speed = speed;
        }
    }

    // Устанавливаем режим REB для всех врагов
    public void SetREB(eREB mode)
    {
        if (rebList.Count == 0)
            return;
        foreach (var reb in rebList)
        {
            reb.SetMode(mode);
        }
    }

    // Устанавливаем скорость всех врагов
    public void SetSpeed(float value)
    {
        if (newVersion)
        {
            if (carAI.Count == 0)
                return;
        }
        else
        {
            if (agents.Count == 0)
                return;
        }
        StartCoroutine(SetSpeedIEnum(value));
    }

    // Корутин для установки скорости с небольшой задержкой
    public IEnumerator SetSpeedIEnum(float value)
    {
        if (newVersion)
        {
            foreach (var enemy in carAI)
            {
                enemy.desiredSpeed = 20;  // Устанавливаем временную скорость
            }
            yield return new WaitForSeconds(1);
            foreach (var enemy in carAI)
            {
                enemy.desiredSpeed = value;
            }
        }
        else
        {
            foreach (var enemy in agents)
            {
                enemy.speed = 20;  // Устанавливаем временную скорость
            }
            yield return new WaitForSeconds(1);
            foreach (var enemy in agents)
            {
                enemy.speed = value;
            }
        }
    }
}
