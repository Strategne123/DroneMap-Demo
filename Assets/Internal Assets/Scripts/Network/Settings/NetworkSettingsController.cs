using System;
using Mirror;


public class NetworkSettingsController : NetworkBehaviour
{
    [SyncVar] private NetworkSettingsData _data;

    private void Awake()
    {
        DontDestroyOnLoad(this);
    }


    public void GetDataFromUI()
    {
        _data = UISettingsController.Instance.GetData();
    }

    public NetworkSettingsData GetData() => _data;
}

[Serializable]
public class NetworkSettingsData
{
    public int CurrAmmoIndex;
    public int CurrBatteryIndex;
    public int CurrWindIndex;
    public int CountCarsIndex;
    public int CurrCarSpeed;
    public int CurrREB;
    public int TimeIndex;
    public int CurrDrone;
}

public static class NetworkSettingsDataReaderWriter
{
    public static void WriteNetworkSettingsData(this NetworkWriter writer, NetworkSettingsData data)
    {
        writer.WriteInt(data.CurrAmmoIndex);
        writer.WriteInt(data.CurrBatteryIndex);
        writer.WriteInt(data.CurrWindIndex);
        writer.WriteInt(data.CountCarsIndex);
        writer.WriteInt(data.CurrCarSpeed);
        writer.WriteInt(data.CurrREB);
        writer.WriteInt(data.TimeIndex);
        writer.WriteInt(data.CurrDrone);
    }
     
    public static NetworkSettingsData ReadNetworkSettingsData(this NetworkReader reader)
    {
        NetworkSettingsData data = new NetworkSettingsData();
        data.CurrAmmoIndex = reader.ReadInt();
        data.CurrBatteryIndex = reader.ReadInt();
        data.CurrWindIndex = reader.ReadInt();
        data.TimeIndex = reader.ReadInt();
        data.CurrDrone = reader.ReadInt();
        data.CountCarsIndex = reader.ReadInt();
        data.CurrCarSpeed = reader.ReadInt();
        data.CurrREB = reader.ReadInt();
        return data;
    }
}
