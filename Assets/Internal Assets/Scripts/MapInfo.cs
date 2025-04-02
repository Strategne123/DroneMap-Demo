using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelTypeInfo
{
    public string levelType; // Тип уровня
    public bool isEnabled;   // Флаг активности
}

public class MapInfo : MonoBehaviour
{
    public int numLevel;
    public string caption;
    public Sprite sprite;
    public Vector3 position;
    public Quaternion rotation;

    // Список типов уровней
    public List<LevelTypeInfo> levelTypes = new List<LevelTypeInfo>();



    // Метод для проверки, используется ли уже данный тип уровня
    public bool IsLevelTypeUsed(string levelType)
    {
        foreach (var level in levelTypes)
        {
            if (level.levelType == levelType)
                return true;
        }
        return false;
    }

    
}
