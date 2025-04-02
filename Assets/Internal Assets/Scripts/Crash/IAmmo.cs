using UnityEngine;

public interface IAmmo 
{
    public eAmmo GetAmmoType();
    public Vector3 GetPosition();
    public float GetExplosionRadius();

}
