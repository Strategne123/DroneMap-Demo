using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class DroneStartPositioner : NetworkBehaviour
{
    public List<Transform> _startPositions;

    [SyncVar(hook = nameof(OnPositionsUpdated))]
    private SyncList<Transform> _positionsPool = new SyncList<Transform>();

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Инициализация только на сервере
        _positionsPool.AddRange(_startPositions);
    }

    [Server]
    public virtual Transform GetNewTransform(VirtualDrone drone)
    {
        if (_positionsPool.Count == 0)
        {
            Debug.LogWarning("No available start positions!");
            return transform; // Возвращаем позицию по умолчанию
        }
        int index = Random.Range(0, _positionsPool.Count);
        Transform position = _positionsPool[index];
        _positionsPool.RemoveAt(index);
        return position;
    }

    // Хук для синхронизации изменений
    private void OnPositionsUpdated(SyncList<Transform>.Operation op, int index, Transform oldItem, Transform newItem)
    {
        // Обновление визуального представления (если нужно)
    }
}
