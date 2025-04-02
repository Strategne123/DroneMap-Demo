
using Mirror;
using System;
using UnityEngine;

public class NetworkCar : NetworkBehaviour
{
    [SerializeField] private MachineDamage _machineDamage;

    [SyncVar(hook = nameof(SetREB))] public int rebValue;
    [SyncVar(hook = nameof(SetSpeed))] public float speed;

    private void Start()
    {
        _machineDamage.OnGotDamage += (() =>
        {
            if (isServer)
            {
                RpcGotDamage();
            }
            else if (isClient)
            {
                print("client got damage");
                CmdGotDamage();
            }
        });
    }

    private void SetREB(int oldValue, int newValue)
    {
        REB reb = GetComponentInChildren<REB>();
        if (reb != null)
        {
            //reb.streight = newValue;
            //reb.signalRadius.radius = newValue * 140;
            reb.SetMode((eREB)newValue);
        }

    }

    private void SetSpeed(float oldValue, float newValue)
    {
        GetComponent<CarAIController>().desiredSpeed = newValue;
    }

    [Command(requiresAuthority = false)]
    public void CmdGotDamage()
    {
        print("cmd got damage");
        _machineDamage.DestroyEffectEnabling();
        RpcGotDamage();
    }

    // Метод, который будет вызываться на сервере и исполняться на клиентах
    [ClientRpc]
    void RpcGotDamage()
    {
        _machineDamage.DestroyEffectEnabling();
    }
}
