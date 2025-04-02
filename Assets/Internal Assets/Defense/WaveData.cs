using UnityEngine;

[System.Serializable]
public class WaveData
{
    [Tooltip("Сколько путей Easy-сложности заспавнить в этой волне")]
    public int easyCount;

    [Tooltip("Сколько путей Medium-сложности заспавнить в этой волне")]
    public int mediumCount;

    [Tooltip("Сколько путей Hard-сложности заспавнить в этой волне")]
    public int hardCount;
}
