using UnityEngine;

public class REB : MonoBehaviour
{
    private LevelOfGlitches levelGlitches;
    private MachineDamage machineDamage;

    private float strongLevelOfGlitches = 0.5f;
    private SphereCollider weakREBZone; //50
    [SerializeField] private SphereCollider strongREBZone; //15 
    [SerializeField] private SphereCollider maxREBZone; //8

    private float weakREB, strongREB, maxREB;

    private void Awake()
    {
        levelGlitches = FindObjectOfType<LevelOfGlitches>();
        machineDamage = transform.parent.GetComponent<MachineDamage>();
        weakREBZone = GetComponent<SphereCollider>();

        SetMode(eREB.Weak);
        strongREBZone.enabled = false;
        maxREBZone.enabled = false;
    }

    public void SetMode(eREB mode)
    {
        weakREBZone.enabled = true;
        switch (mode)
        {
            case eREB.None:
                weakREBZone.enabled = false;
                break;
            case eREB.Weak:
                weakREBZone.radius = 25;
                strongREBZone.radius = 7.5f;
                maxREBZone.radius = 0;
                break;
            case eREB.Powerfull:
                weakREBZone.radius = 50;
                strongREBZone.radius = 15;
                maxREBZone.radius = 7.5f;
                break;
        }

        weakREB = weakREBZone.radius * weakREBZone.transform.lossyScale.x;
        strongREB = strongREBZone.radius * strongREBZone.transform.lossyScale.x;
        maxREB = maxREBZone.radius * maxREBZone.transform.lossyScale.x;
    }

    private void OnTriggerStay(Collider collision)
    {
        if (machineDamage != null)
        {
            if (machineDamage.isExp) return;
        }
        if (collision.GetComponent<VirtualDrone>() && levelGlitches != null)
        {
            if (collision.GetComponent<VirtualDrone>().IsExp()) return;
            var dist = Vector3.Distance(collision.transform.position, weakREBZone.transform.position);

            float REB;
            if (dist > weakREB)
            {
                REB = 0f;
            }
            else if (dist > strongREB)
            {
                // Между weakREB и strongREB
                float t = 1 - (dist - strongREB) / (weakREB - strongREB);
                REB = Mathf.Lerp(0f, strongLevelOfGlitches, t);
            }
            else if (dist > maxREB)
            {
                // Между strongREB и maxREB
                float t = 1 - (dist - maxREB) / (strongREB - maxREB);
                REB = Mathf.Lerp(strongLevelOfGlitches, 1f, t);
            }
            else
            {
                REB = 1f;
            }

            levelGlitches.Level += REB;//Mathf.Pow(REB, Mathf.Exp(0.75f));
        }
    }

}
