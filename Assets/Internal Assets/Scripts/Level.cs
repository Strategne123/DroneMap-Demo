using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour
{
    public enum Difficulty
    {
        Легко,
        Нормально,
        Сложно
    }

    private bool _canCatchInterference;
    private NetworkAttackController _attackController;

    public List<Material> skyboxMaterials = new List<Material>();
    private Light mainLight;
    public Color dayColor;
    public Color nightColor;
    public Vector3 eulerDayAngle;
    public Vector3 eulerNightAngle;
    public Color dayFog, nightFog;
    public int minFogDistance, maxFogDistance;
    public Difficulty currentDifficulty = Difficulty.Легко;
    public bool isPlaying = true;
    public float globalTime;
    public bool canCrash;
    public bool canCatchInterference;
    public bool thrustEnable = true;
    public bool yawEnable = true;
    public bool pitchEnable = true;
    public bool rollEnable = true;
    public string levelCaption;
    public bool HasPilot = true;
    [HideInInspector] public int levelNetId;
    public bool outlineEnabled;
    private VirtualDrone _drone;
    VirtualAccountManager accountManager;
    VirtualAccount account;

    private readonly Dictionary<Difficulty, float> difficultyRecordMap = new()
{
    { Difficulty.Легко, 1000000 },
    { Difficulty.Нормально, 1000000 },
    { Difficulty.Сложно, 1000000 }
};

    public VirtualDrone Drone
    {
        get { return _drone; }
        set
        {
            _drone = value;
            _drone.SetOutline(outlineEnabled);
        }
    }

    public List<eTypeSettings> allowedSettings = new List<eTypeSettings>() { eTypeSettings.Wind, eTypeSettings.Drone, eTypeSettings.Time, eTypeSettings.Ammo, eTypeSettings.Difficulty, eTypeSettings.Machines, eTypeSettings.CountMachines, eTypeSettings.REB, eTypeSettings.Battery };

    public bool isRaceLevel { get; protected set; }

    public bool IsActive()
    {
        if (Drone == null)
            return false;
        return Drone.level == this;
    }

    public void Activate()
    {
        Drone.level = this;
        isPlaying = true;
    }

    private void Awake()
    {
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = minFogDistance;
        RenderSettings.fogEndDistance = maxFogDistance;
        _attackController = GetComponent<NetworkAttackController>();
        accountManager = FindObjectOfType<VirtualAccountManager>();
        account = FindObjectOfType<VirtualAccount>();

    }

    public void CleanUp()
    {
        // Очистка списков
        skyboxMaterials.Clear();
        allowedSettings.Clear();

        _attackController = null;
        _drone = null;

        RenderSettings.skybox = null;
        RenderSettings.fogColor = Color.black;

        PlayerPrefs.DeleteKey(levelCaption + "Settings");
        PlayerPrefs.DeleteKey(levelCaption + "BoolSettings");

        Destroy(gameObject);
    }


    public virtual string ShowInfo()
    {
        return null;
    }

    public void SetMachinesCount(int count)
    {
        if (_attackController != null)
        {
            _attackController.SpawnCars(count);
        }
    }

    private void RefreshShaders(GameObject rootObject)
    {
        Renderer[] renderers = rootObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material != null && material.shader != null)
                {
                    Shader shader = Shader.Find(material.shader.name);
                    if (shader != null)
                    {
                        material.shader = shader;
                    }
                    else
                    {
                        Debug.LogError($"Шейдер {material.shader.name} не найден!");
                    }
                }
            }
        }
    }

    public virtual void Init()
    {

    }

    public void SetTime(int num)
    {
        var lights = FindObjectsOfType<Light>();
        foreach (var light in lights)
        {
            if (light.type == LightType.Directional)
            {
                mainLight = light;
                break;
            }
        }
        if (skyboxMaterials.Count > 0)
        {
            RenderSettings.skybox = skyboxMaterials[num];
            if (num == 0)
            {
                SetDay();
            }
            else if (num == 1)
            {
                SetNight();
            }
        }

        DynamicGI.UpdateEnvironment();
        RefreshShaders(gameObject);
    }

    private void SetDay()
    {
        RenderSettings.fogColor = dayFog;
        mainLight.color = dayColor;
        mainLight.transform.eulerAngles = eulerDayAngle;
    }

    private void SetNight()
    {
        RenderSettings.fogColor = nightFog;
        mainLight.color = nightColor;
        mainLight.transform.eulerAngles = eulerNightAngle;
    }

    public void SetREB(eREB mode)
    {
        if (_attackController != null)
        {
            _attackController.SetREB(mode);
        }
    }

    public void SetEnemySpeed(float value)
    {
        if (_attackController != null)
        {
            _attackController.SetSpeed(value);
        }
    }

    private void InitializeDifficultyRecordMap(Record record)
    {
        difficultyRecordMap[Difficulty.Легко] = record.simpleRecord;
        difficultyRecordMap[Difficulty.Нормально] = record.mediumRecord;
        difficultyRecordMap[Difficulty.Сложно] = record.hardRecord;
    }


    public virtual string ShowInfo(Difficulty currentDifficulty, float endTime)
    {
        var record = accountManager.GetRecord(levelCaption) ?? account.AddLevels(accountManager.GetCurrentAccount(), levelCaption);
        InitializeDifficultyRecordMap (record);
        string currentTimeString = endTime.ToString("F2");
        string info = "Время: " + currentTimeString + " сек\n";
        if(endTime < difficultyRecordMap[currentDifficulty])
        {
            UpdateRecord(currentDifficulty, endTime);
        }
        foreach (Difficulty diff in Enum.GetValues(typeof(Difficulty)))
        {
            float recordTime = difficultyRecordMap[diff];
            string recordString = recordTime < 1000000 ? recordTime.ToString("F2") : "Нет";
            info += "Рекорд для " + diff.ToString() + ": " + recordString + " сек\n";
        }
        
        return info;
    }

    public virtual void SetDifficulty(int difficulty)
    {
    }

    protected void UpdateRecord(Difficulty difficulty, float time)
    {
        var record = accountManager.GetRecord(levelCaption) ?? account.AddLevels(accountManager.GetCurrentAccount(), levelCaption);
        if (time < difficultyRecordMap[difficulty] || difficultyRecordMap[difficulty] == 0)
        {
            difficultyRecordMap[difficulty] = time;
            SaveRecordToAccount(record, difficulty, time);
        }
    }

    private void SaveRecordToAccount(Record record, Difficulty difficulty, float time)
    {
        switch (difficulty)
        {
            case Difficulty.Легко: record.simpleRecord = time; break;
            case Difficulty.Нормально: record.mediumRecord = time; break;
            case Difficulty.Сложно: record.hardRecord = time; break;
        }
        accountManager.UpdateRecords(record);
    }

    public void Serialize()
    {
        if (!Drone.isOwned)
            return;
        if (Drone.GetCountCarousels() == 0)
            return;
        var result = "";
        foreach (var item in Drone.GetCarousels())
        {
            result += item.Value.currentIndex + " ";
        }
        PlayerPrefs.SetString(levelCaption + "Settings", result);
    }

    public void Deserialize()
    {
        if (!Drone.isOwned)
            return;
        if (!PlayerPrefs.HasKey(levelCaption + "Settings"))
            return;
        var result = PlayerPrefs.GetString(levelCaption + "Settings");
        var indexes = result.Split(' ');
        int i = 0;
        foreach (var item in Drone.GetCarousels())
        {
            try
            {
                Drone.SetControllerParameter(item.Key, int.Parse(indexes[i++]));
                UISettingsController.Instance.SetData();
            }
            catch { }
        }
    }

    public void BoolSerialize()
    {
        if (!Drone.isOwned)
            return;
        if (Drone.GetCountBoolVariables() == 0)
            return;
        var result = "";
        foreach (var item in Drone.GetBoolVariables())
        {
            result += item.Value.isOn ? "1" : "0";
        }
        PlayerPrefs.SetString(levelCaption + "BoolSettings", result);
    }

    public void BoolDeserialize()
    {
        if (!Drone.isOwned)
            return;
        if (!PlayerPrefs.HasKey(levelCaption + "BoolSettings"))
            return;
        var result = PlayerPrefs.GetString(levelCaption + "BoolSettings");
        int i = 0;
        foreach (var item in Drone.GetBoolVariables())
        {
            var temp = result[i++] == '1';
            item.Value.isOn = temp;
        }
    }
}
