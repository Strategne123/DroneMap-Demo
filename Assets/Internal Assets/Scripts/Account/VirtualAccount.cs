using UnityEngine;

public abstract class VirtualAccount: MonoBehaviour
{
    public abstract Record AddLevels(VirtualAccount account, string levelCaption);
}
