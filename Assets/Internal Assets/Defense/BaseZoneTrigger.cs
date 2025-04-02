using UnityEngine;
using Mirror;

public class BaseZoneTrigger : NetworkBehaviour
{
    [SerializeField] private DefenseModeController defenseModeController;

    /// <summary>
    /// Вызывается, когда объект заходит в триггер.
    /// </summary>
    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        // Только на сервере (ServerCallback гарантирует вызов только на сервере)
        if (!enabled) return;

        // Проверяем, что это именно машина. Можно проверять по тегу, компоненту и т.д.
        CarController car = other.GetComponent<CarController>();
        if (car != null)
        {
            Debug.Log($"Машина {car.name} достигла базы!");

            // Сообщаем контроллеру обороны, что базе нанесён урон.
            defenseModeController.TakeDamage(defenseModeController.damagePerCar);

            // Уведомляем контроллер, что машина завершила свою жизнь (достигла базы).
            defenseModeController.OnVehicleRemoved();

            // Уничтожаем машину, чтобы она не повторяла урон
            NetworkServer.Destroy(car.gameObject);
        }
    }
}
