using UnityEngine;

public abstract class VirtualAccountManager : MonoBehaviour
{
    public abstract VirtualAccount GetCurrentAccount();

    public abstract Record GetRecord(string caption);

    public abstract void UpdateRecords(Record record);
}
