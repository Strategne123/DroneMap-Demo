using Mirror;
using System;
using UnityEngine;
using System.Collections;

public class DroneNetworkSettings : MonoBehaviour
{
    public bool isOwned = false;
    public event Action OnInitialized;
    public bool isInitialized { get; private set; }
    private SettingsController _settingsController;
    private NetworkSettingsController _networkSettingsController; 

    private void Start()
    {
        _settingsController = FindObjectOfType<SettingsController>();
        StartCoroutine(Initialization());
    }

    private IEnumerator Initialization()
    {
        while (!isInitialized)
        {
            _networkSettingsController = FindObjectOfType<NetworkSettingsController>(true);
            Init();
            yield return new WaitForEndOfFrame();
        }
    }

    private void Init()
    {
        if (_settingsController == null || _networkSettingsController == null || _settingsController.drone == null)
            return;
        isOwned = GetComponentInParent<NetworkBehaviour>().isOwned;
        NetworkSettingsData data = _networkSettingsController.GetData();
        //Это работает только для одиночки
        if (data == null)
            return;
        _settingsController.SetDrone(data.CurrDrone);
        _settingsController.SetAmmo(data.CurrAmmoIndex);
        _settingsController.SetBattery(data.CurrBatteryIndex);
        _settingsController.SetWind(data.CurrWindIndex);
        _settingsController.SetTime(data.TimeIndex);
        _settingsController.SetCountMachines(data.CountCarsIndex);
        _settingsController.SetMachines(data.CurrCarSpeed);
        _settingsController.SetREB(0);
        isInitialized = true;
        OnInitialized?.Invoke();
    }

    public int GetCountCars() => _networkSettingsController.GetData().CountCarsIndex switch
    {
        1 => 4,
        2 => 12,
        _ => 0,
    };

    public float GetCarSpeed() => _networkSettingsController.GetData().CurrCarSpeed switch
    {
        1 => 20f,
        2 => 50f,
        _ => 0.001f,
    };

    public int GetCarREB() => _networkSettingsController.GetData().CurrREB;
}
