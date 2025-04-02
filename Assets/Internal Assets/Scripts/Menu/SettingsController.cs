using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsController : MonoBehaviour
{
    public WindController windZone;
    public VirtualDrone drone { get; set; }
    public Dictionary<eTypeSettings, Carousel> carousels = new Dictionary<eTypeSettings, Carousel>();
    public Dictionary<eBoolVariables, Toggle> boolVariables = new Dictionary<eBoolVariables, Toggle>();

    public void Init()
    {
        windZone = FindObjectOfType<WindController>();
        var children = GetComponentsInChildren<Carousel>();
        carousels.Clear();
        boolVariables.Clear();
        for (var i = children.Length - 1; i >= 0; i--)
        {
            var carousel = children[i];
            carousel.Init();
            carousel.settingsController = this;

            if (!drone.level.allowedSettings.Contains(carousel.typeSetting))
            {
                carousel.transform.parent.parent.gameObject.SetActive(false);
            }
            else
            {
                carousels.Add(carousel.typeSetting, carousel);
            }
            SetParameter(carousel);
        }
        drone.level.Deserialize();
        int j = 0;
        var boolChildren = GetComponentsInChildren<Toggle>();
        foreach (var child in boolChildren)
        {
            boolVariables.Add((eBoolVariables)j++, child);
        }
        drone.level.BoolDeserialize();
    }


    public void SetParameter(Carousel carousel)
    {
        switch (carousel.typeSetting)
        {
            case eTypeSettings.Wind:
                SetWind(carousel.currentIndex);
                break;
            case eTypeSettings.Ammo:
                SetAmmo(carousel.currentIndex);
                break;
            case eTypeSettings.Battery:
                SetBattery(carousel.currentIndex);
                break;
            case eTypeSettings.Difficulty:
                SetDifficulty(carousel.currentIndex);
                break;
            case eTypeSettings.CountMachines:
                SetCountMachines(carousel.currentIndex);
                break;
            case eTypeSettings.REB:
                SetREB(carousel.currentIndex);
                break;
            case eTypeSettings.Machines:
                SetMachines(carousel.currentIndex);
                break;
            case eTypeSettings.Drone:
                SetDrone(carousel.currentIndex);
                break;
            case eTypeSettings.Time:
                SetTime(carousel.currentIndex);
                break;
            case eTypeSettings.CarRatio:
            case eTypeSettings.TankRatio:
            case eTypeSettings.ArmoredVehicleRatio:
                //UpdateVehicleRatios();
                break;
        }
    }


    public void SetParameter(eTypeSettings typeSettings, int value)
    {
        switch (carousels[typeSettings].typeSetting)
        {
            case eTypeSettings.Wind:
                SetWind(value);
                break;
            case eTypeSettings.Ammo:
                SetAmmo(value);
                break;
            case eTypeSettings.Battery:
                SetBattery(value);
                break;
            case eTypeSettings.Difficulty:
                SetDifficulty(value);
                break;
            case eTypeSettings.CountMachines:
                SetCountMachines(value);
                break;
            case eTypeSettings.REB:
                SetREB(value);
                break;
            case eTypeSettings.Machines:
                SetMachines(value);
                break;
            case eTypeSettings.Drone:
                SetDrone(value);
                break;
            case eTypeSettings.Time:
                SetTime(value);
                break;
            case eTypeSettings.CarRatio:
            case eTypeSettings.TankRatio:
            case eTypeSettings.ArmoredVehicleRatio:
                break;
        }

        carousels[typeSettings].UpdateAll(value);
    }

    private void UpdateVehicleRatios()
    {
        if (carousels.ContainsKey(eTypeSettings.CarRatio) &&
            carousels.ContainsKey(eTypeSettings.TankRatio) &&
            carousels.ContainsKey(eTypeSettings.ArmoredVehicleRatio))
        {
            float carRatio = carousels[eTypeSettings.CarRatio].currentIndex / 10f;
            float tankRatio = carousels[eTypeSettings.TankRatio].currentIndex / 10f;
            float armoredVehicleRatio = carousels[eTypeSettings.ArmoredVehicleRatio].currentIndex / 10f;

            FindObjectOfType<NetworkAttackController>().SetRatios(carRatio, tankRatio, armoredVehicleRatio);
        }
        else
        {
            Debug.LogError("One of the vehicle ratio carousels is missing!");
        }
    }

    public void SetWind(int currentIndex)
    {
        if (windZone == null)
            return;
        switch ((eWind)currentIndex)
        {
            case eWind.None:
                windZone.maxWindStrength = 0;
                break;
            case eWind.Light:
                windZone.maxWindStrength = 4;
                windZone.changeInterval = 30;
                break;
            case eWind.Medium:
                windZone.maxWindStrength = 7;
                windZone.changeInterval = 20;
                break;
            case eWind.Strong:
                windZone.maxWindStrength = 9;
                windZone.changeInterval = 15;
                break;
        }
        windZone.timer = 0;
    }

    public void SetWind(Carousel carousel)
    {
        SetWind(carousel.currentIndex);
    }

    public void SetAmmo(int currentIndex)
    {
        switch ((eAmmo)currentIndex)
        {
            case eAmmo.None:
                drone.SelectAmmo(eAmmo.None);
                break;
            case eAmmo.ArmorPiercing:
                drone.SelectAmmo(eAmmo.ArmorPiercing);
                break;
            case eAmmo.HighExplosive:
                drone.SelectAmmo(eAmmo.HighExplosive);
                break;
        }
    }

    public void SetAmmo(Carousel carousel)
    {
        SetAmmo(carousel.currentIndex);
    }

    public void SetBattery(int currentIndex)
    {
        switch ((eBattery)currentIndex)
        {
            case eBattery.Arkade:
                drone.SetBatteryLifeTime(1000000);
                break;
            case eBattery.Standart:
                drone.SetBatteryLifeTime(600);
                break;
            case eBattery.Hardcore:
                drone.SetBatteryLifeTime(300);
                break;
        }
    }

    public void SetBattery(Carousel carousel)
    {
        SetBattery(carousel.currentIndex);
    }

    public void SetDifficulty(int currentIndex)
    {
        drone.level.SetDifficulty(currentIndex);
    }

    public void SetDifficulty(Carousel carousel)
    {
        drone.level.SetDifficulty(carousel.currentIndex);
    }

    public void SetCountMachines(int currentIndex)
    {
        switch ((eCountMachines)currentIndex)
        {
            case eCountMachines.None:
                drone.level.SetMachinesCount(0);
                break;
            case eCountMachines.Few:
                drone.level.SetMachinesCount(4);
                break;
            case eCountMachines.Much:
                drone.level.SetMachinesCount(12);
                break;
        }
    }

    public void SetCountMachines(Carousel carousel)
    {
        SetCountMachines(carousel.currentIndex);
    }

    public void SetREB(Carousel carousel)
    {
        SetREB(carousel.currentIndex);
    }

    public void SetREB(int currentIndex)
    {
        drone.level.SetREB((eREB)currentIndex);
    }

    public void SetMachines(Carousel carousel)
    {
        SetMachines(carousel.currentIndex);
    }

    public void SetMachines(int currentIndex)
    {
        switch ((eMachines)currentIndex)
        {
            case eMachines.Static:
                drone.level.SetEnemySpeed(0.001f);
                break;
            case eMachines.Slow:
                drone.level.SetEnemySpeed(20);
                break;
            case eMachines.Fast:
                drone.level.SetEnemySpeed(50);
                break;
        }
    }

    public void SetDrone(Carousel carousel)
    {
        drone.SetDroneBody(carousel.currentIndex);
    }

    public void SetDrone(int currentIndex)
    {
        drone.SetDroneBody(currentIndex);
    }

    public void SetTime(Carousel carousel)
    {
        SetTime(carousel.currentIndex);
    }

    public void SetTime(int currentIndex)
    {
        switch ((eTime)currentIndex)
        {
            case eTime.Day:
                drone.level.SetTime(currentIndex);
                break;
            case eTime.Night:
                drone.level.SetTime(currentIndex);
                break;
        }
    }

}

public enum eWind
{
    None,
    Light,
    Medium,
    Strong,
}

public enum eTypeSettings
{
    Wind,
    Drone,
    Time,
    Ammo,
    Difficulty,
    Machines,
    CountMachines,
    REB,
    Battery,
    CarRatio,
    TankRatio,
    ArmoredVehicleRatio
}


public enum eAmmo
{
    None,
    ArmorPiercing,
    HighExplosive,
}

public enum eBattery
{
    Arkade,
    Standart,
    Hardcore,
}

public enum eDifficulty
{
    Easy,
    Medium,
    Difficult,
}

public enum eCountMachines
{
    None,
    Few,
    Much,
}

public enum eREB
{
    None,
    Weak,
    Powerfull,
}

public enum eMachines
{
    Static,
    Slow,
    Fast,
}

[Serializable]
public enum eBoolVariables
{
    Sticks,
    Glitches,
    UI,
    ScanLines,
    RGB,
    BadTV,
    WhiteNoise,
    Ripple,
    FishEye,
}

public enum eDrone
{
    SmallDrone,
    MiddleDrone,
}

public enum eTime
{
    Day,
    Night,
}

