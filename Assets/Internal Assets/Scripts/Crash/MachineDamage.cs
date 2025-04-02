using Mirror;
using System;
using System.Collections;
using UnityEngine;

public class MachineDamage : NetworkBehaviour, ICrashable
{
    [SerializeField] private Machines machineType;
    [SerializeField] private GameObject brokeEffect;
    [SerializeField] private GameObject smokeEffect;
    [SerializeField] private GameObject fireEffect;

    [SyncVar(hook = nameof(OnIsExpChanged))]
    public bool isExp = false;

    public event Action OnGotDamage;

    private bool isStopped = false;

    public void Crash(IAmmo ammo)
    {
        if (!isServer)
        {
            CmdRequestCrash();
            return;
        }

        if (isExp) return; // Если уже взорвана, ничего не делаем

        isExp = true;

        // Отображаем эффекты на клиентах
        RpcShowExplosionEffects(ammo != null ? ammo.GetAmmoType() : eAmmo.HighExplosive);

        // Применяем физическое воздействие
        var rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.mass = 1300;

        if (ammo != null)
        {
            rb.AddExplosionForce(10000, ammo.GetPosition(), ammo.GetExplosionRadius());
        }
        else
        {
            rb.AddExplosionForce(10000, transform.position, 5f); // Используем значения по умолчанию
        }

        // Уведомляем контроллер обороны о том, что машина уничтожена
        DefenseModeController defCtrl = FindObjectOfType<DefenseModeController>();
        if (defCtrl != null)
            defCtrl.OnVehicleRemoved();

        OnGotDamage?.Invoke();

        // Запускаем корутину удаления объекта через 5 секунд
        StartCoroutine(DelayedDestroy(5f));
    }

    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (isServer)
            NetworkServer.Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isServer)
        {
            // Обрабатываем столкновение с Ammo на сервере
            IAmmo ammo = collision.gameObject.GetComponent<IAmmo>();
            if (ammo != null && !isExp)
            {
                Crash(ammo);
            }
        }
        else if (isClient)
        {
            // Обрабатываем столкновение с дроном на клиенте
            if (collision.gameObject.CompareTag("Drone") && !isExp)
            {
                // Отправляем команду на сервер для взрыва машины
                CmdRequestCrash();
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdRequestCrash()
    {
        if (!isExp)
        {
            Crash(null); // Вызываем Crash без Ammo
        }
    }

    void OnIsExpChanged(bool oldValue, bool newValue)
    {
        if (newValue)
        {
            if (!fireEffect.activeSelf)
            {
                fireEffect.SetActive(true);
            }
        }
    }

    [ClientRpc]
    void RpcShowExplosionEffects(eAmmo ammoType)
    {
        if ((int)ammoType >= (int)machineType)
        {
            fireEffect.SetActive(true);
        }
        else
        {
            smokeEffect.SetActive(true);
        }
    }

    public void DestroyEffectEnabling()
    {
        isExp = true;
        if (!fireEffect.activeSelf)
        {
            fireEffect.SetActive(true);
        }
    }
}
