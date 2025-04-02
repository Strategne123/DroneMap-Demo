using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AtackLevel : Level
{
    private List<MachineDamage> enemies = new List<MachineDamage>();
    private HashSet<MachineDamage> enemySet = new HashSet<MachineDamage>();
    private float startTime;
    private string timeStr;
    private float currentSec, currentMin;
    private void Start()
    {
        StartCoroutine(UpdateEnemyList());
        startTime = Time.time;
    }

    private IEnumerator UpdateEnemyList()
    {
        while (true)
        {
            MachineDamage[] currentEnemies = FindObjectsOfType<MachineDamage>();

            foreach (var enemy in currentEnemies)
            {
                if (!enemySet.Contains(enemy))
                {
                    enemies.Add(enemy);
                    enemySet.Add(enemy);
                }
            }

            // Удаляем устаревших врагов из enemySet		
            HashSet<MachineDamage> enemiesToRemove = new HashSet<MachineDamage>(enemySet);
            foreach (var enemy in enemiesToRemove)
            {
                if (!currentEnemies.Contains(enemy))
                {
                    enemySet.Remove(enemy);
                    enemies.Remove(enemy);
                }
            }
            // Обновление списка каждые 2 секунды
            yield return new WaitForSeconds(2f);
        }
    }

    override public string ShowInfo()
    {
        //print("show info attack");
        int count = enemies.Count;
        if (count == 0) return "";

        int destroyed = 0;
        foreach (var enemy in enemies)
        {
            if (enemy.isExp)
            {
                destroyed++;
            }
        }

        if (destroyed == count)
        {
            if (Input.GetButtonUp("Fire1"))
            {
                Drone.ExitToScene("Menu3D");
            }
            return $"Миссия выполнена! Ваше время: {timeStr}\nНажмите клавишу действия\n для выхода в меню";
        }
        currentSec = Time.time - startTime;
        TimeSpan time = TimeSpan.FromSeconds(currentSec);

        timeStr = time.ToString(@"mm\:ss");
        return $"Уничтожено: {destroyed}/{count}\nВремя: {timeStr}";
    }
}
