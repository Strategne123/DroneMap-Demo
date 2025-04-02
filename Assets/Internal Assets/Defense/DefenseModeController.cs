using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Invector.vCharacterController;

public class DefenseModeController : NetworkBehaviour
{
    [Header("Wave Settings")]
    public List<WaveData> waves = new List<WaveData>();

    [Header("Paths by Difficulty")]
    public List<PathDrawer> easyPaths = new List<PathDrawer>();
    public List<PathDrawer> mediumPaths = new List<PathDrawer>();
    public List<PathDrawer> hardPaths = new List<PathDrawer>();

    [Header("Base and Zone Settings")]
    public GameObject baseObject;
    [Tooltip("Коллайдер триггера базы (под него будет отдельный скрипт)")]
    public Collider zoneCollider;
    public int maxBaseHP = 100;

    [Tooltip("Урон базе от одной машины")]
    public int damagePerCar = 10;

    [Header("Live Status")]
    [SyncVar] public int currentBaseHP;

    private int currentWaveIndex = 0;
    private bool defenseModeActive = false;

    // Счётчик машин текущей волны (увеличивается при спавне, уменьшается при уничтожении/достижении базы)
    [SyncVar] private int currentWaveVehicleCount = 0;

    [Header("Debug/Testing")]
    // Если включено, то режим обороны запускается автоматически при спавне префаба
    public bool enableDefenseMode = false;

    [Header("Defense Mode Settings")]
    public Transform defensePlayerPoint;

    [Header("Spawn Settings")]
    [Tooltip("Задержка между спавном машин на одном и том же пути (в секундах)")]
    public float spawnDelay = 1.0f;

    private void Start()
    {
        currentBaseHP = maxBaseHP;

        // Проверяем, что это сервер, т.к. логика обороны выполняется только на сервере
        if (!isServer) return;

        // Если чекбокс enableDefenseMode включён, запускаем режим обороны сразу при спавне
        if (enableDefenseMode)
        {
            Debug.Log("Авто-старт режима обороны включён. Запуск обороны...");
            StartDefenseMode();
        }
    }

    void Update()
    {
        if (!isServer) return; // Только на сервере

        // Если режим обороны не активен, можно его запустить нажатием клавиши O
        if (Input.GetKeyDown(KeyCode.O) && !defenseModeActive)
        {
            StartDefenseMode();
        }

        if (currentBaseHP <= 0)
        {
            Debug.Log("База уничтожена! Игра окончена.");
            EndDefenseMode();
        }
    }

    [Server]
    public void StartDefenseMode()
    {
        if (waves.Count == 0)
        {
            Debug.LogWarning("Нет волн в списке. Добавьте их в инспекторе!");
            return;
        }

        defenseModeActive = true;

        // Находим переключатель режима игрока и переключаем режим
        PlayerModeSwitcher playerModeSwitcher = FindObjectOfType<PlayerModeSwitcher>();
        if (playerModeSwitcher != null)
        {
            playerModeSwitcher.TogglePlayerMode();
            if (defensePlayerPoint != null)
                playerModeSwitcher.MoveToDefensePoint(defensePlayerPoint);
        }
        else
        {
            Debug.LogWarning("PlayerModeSwitcher не найден");
        }

        // Отключаем скрипт vThirdPersonInput у пилота
        vThirdPersonInput pilotInput = FindObjectOfType<vThirdPersonInput>();
        if (pilotInput != null)
        {
            pilotInput.enabled = false;
            Debug.Log("Скрипт vThirdPersonInput отключён для пилота");
        }
        else
        {
            Debug.LogWarning("Скрипт vThirdPersonInput не найден");
        }

        currentWaveIndex = 0;
        Debug.Log("Режим обороны запущен!");
        StartWave(currentWaveIndex);
    }

    [Server]
    private void StartWave(int waveIndex)
    {
        if (waveIndex >= waves.Count)
        {
            Debug.Log("Все волны завершены! Режим обороны завершён.");
            defenseModeActive = false;
            return;
        }

        // Сбрасываем счётчик машин для новой волны
        currentWaveVehicleCount = 0;

        WaveData wave = waves[waveIndex];
        Debug.Log($"Старт волны {waveIndex + 1} из {waves.Count}");

        // Спавним машины для каждого уровня сложности
        SpawnVehiclesOnPaths(easyPaths, wave.easyCount);
        SpawnVehiclesOnPaths(mediumPaths, wave.mediumCount);
        SpawnVehiclesOnPaths(hardPaths, wave.hardCount);

        Debug.Log($"Волна {waveIndex + 1} запущена: Easy={wave.easyCount}, Medium={wave.mediumCount}, Hard={wave.hardCount}");
    }

    /// <summary>
    /// Спавнит заданное количество машин на случайно выбранных путях из списка.
    /// Если на одном пути требуется спавнить несколько машин, между спавнами будет задержка.
    /// </summary>
    /// <param name="pathList">Список путей для спавна</param>
    /// <param name="count">Сколько машин нужно заспавнить</param>
    [Server]
    private void SpawnVehiclesOnPaths(List<PathDrawer> pathList, int count)
    {
        if (pathList == null || pathList.Count == 0)
        {
            Debug.LogWarning("Нет доступных путей для этой сложности!");
            return;
        }

        // Для каждого пути будем считать, сколько раз на него запланирован спавн
        Dictionary<PathDrawer, int> spawnCountForPath = new Dictionary<PathDrawer, int>();

        for (int i = 0; i < count; i++)
        {
            // Выбираем случайный путь из списка
            PathDrawer randomPath = pathList[Random.Range(0, pathList.Count)];

            // Определяем порядковый номер спавна для данного пути
            int spawnNumber = 1;
            if (spawnCountForPath.ContainsKey(randomPath))
            {
                spawnNumber = spawnCountForPath[randomPath] + 1;
                spawnCountForPath[randomPath] = spawnNumber;
            }
            else
            {
                spawnCountForPath[randomPath] = spawnNumber;
            }

            // Вычисляем задержку: первая машина спавнится без задержки, вторая через spawnDelay, третья через 2 * spawnDelay и т.д.
            float delay = spawnDelay * (spawnNumber - 1);

            // Запускаем корутину спавна машины с задержкой
            StartCoroutine(SpawnVehicleAfterDelay(randomPath, delay));
        }
    }

    /// <summary>
    /// Корутина, которая ждёт заданное время и затем спавнит машину на указанном пути.
    /// После спавна увеличивается счётчик машин текущей волны.
    /// </summary>
    /// <param name="path">Путь для спавна</param>
    /// <param name="delay">Задержка в секундах</param>
    /// <returns></returns>
    private IEnumerator SpawnVehicleAfterDelay(PathDrawer path, float delay)
    {
        yield return new WaitForSeconds(delay);
        // Спавним машину (предполагается, что метод SpawnVehicle() создаёт и настраивает машину)
        path.SpawnVehicle();
        currentWaveVehicleCount++;
        Debug.Log($"Машин в волне: {currentWaveVehicleCount}");
    }

    /// <summary>
    /// Вызывается, когда машина уничтожается или достигает базы.
    /// Уменьшается счётчик, и если он равен нулю, запускается следующая волна.
    /// </summary>
    [Server]
    public void OnVehicleRemoved()
    {
        currentWaveVehicleCount--;
        Debug.Log($"Осталось машин в волне: {currentWaveVehicleCount}");
        if (currentWaveVehicleCount <= 0)
        {
            Debug.Log("Волна завершена. Переход к следующей...");
            OnWaveCleared();
        }
    }

    [Server]
    private void OnWaveCleared()
    {
        currentWaveIndex++;
        StartWave(currentWaveIndex);
    }

    [Server]
    private void EndDefenseMode()
    {
        defenseModeActive = false;
        Debug.Log("Режим обороны завершён.");
    }

    /// <summary>
    /// Вызывается, когда машина добирается до базы/зоны.
    /// </summary>
    [Server]
    public void TakeDamage(int damage)
    {
        currentBaseHP -= damage;
        if (currentBaseHP < 0)
            currentBaseHP = 0;

        Debug.Log($"База получила урон {damage}. Текущее HP: {currentBaseHP}");
    }
}
