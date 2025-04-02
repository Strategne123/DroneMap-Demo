using System;
using UnityEngine;
using UnityEngine.UI;


public class UISettingsController : Singleton<UISettingsController>
{
    [SerializeField] private Dropdown _ammoDropDown;
    [SerializeField] private Dropdown _batteryDropDown;
    [SerializeField] private Dropdown _windDropDown;
    [SerializeField] private Dropdown _carsCountDropDown;
    [SerializeField] private Dropdown _carsSpeedDropDown;
    [SerializeField] private Dropdown _carsREBDropDown;
    [SerializeField] private Dropdown _timeIndexDropDown;
    [SerializeField] private Dropdown _droneIndexDropDown;

    private NetworkSettingsData _data;

    private void Init()
    {
        _data = new NetworkSettingsData();
        _ammoDropDown.onValueChanged?.AddListener((value => _data.CurrAmmoIndex = value));
        _batteryDropDown.onValueChanged?.AddListener((value => _data.CurrBatteryIndex = value));
        _windDropDown.onValueChanged?.AddListener((value => _data.CurrWindIndex = value));
        _carsCountDropDown.onValueChanged?.AddListener((value => _data.CountCarsIndex = value));
        _carsSpeedDropDown.onValueChanged?.AddListener((value => _data.CurrCarSpeed = value));
        _carsREBDropDown.onValueChanged?.AddListener((value => _data.CurrREB = value));
        _timeIndexDropDown.onValueChanged?.AddListener((value => _data.TimeIndex = value));
        _droneIndexDropDown.onValueChanged?.AddListener((value => _data.CurrDrone = value));
    }

    public void SetData()
    {
        _data = new NetworkSettingsData();
    }
    public NetworkSettingsData GetData() => _data;
}
