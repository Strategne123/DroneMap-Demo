using UnityEngine;

public class WindController : MonoBehaviour
{
    public float windMain = 1;
    public float changeInterval = 5f; // Интервал изменения направления и силы ветра
    public float maxWindStrength = 10f; // Максимальная сила ветра
    public float timer;

    private float targetAngle;
    private float targetWindStrength;
    private WindZone windZone;

    void Start()
    {
        timer = changeInterval;
        windZone = GetComponent<WindZone>();
        SetRandomTarget();
    }

    public void CleanUp()
    {
        windZone = null;
        targetAngle = 0f;
        targetWindStrength = 0f;
        windMain = 0f;
        timer = 0f;
        Destroy(gameObject);
    }


    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SetRandomTarget();
            timer = Random.Range(changeInterval/2,changeInterval);
        }

        float currentAngle = transform.rotation.eulerAngles.y;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, Time.deltaTime * 30f);
        transform.rotation = Quaternion.Euler(0f, newAngle, 0f);

        windMain = Mathf.MoveTowards(windMain, targetWindStrength, Time.deltaTime * 2f);
        windZone.windMain = windMain * 0.5f;
        windZone.windPulseMagnitude = windMain * 0.05f;
        windZone.windTurbulence = windMain>0 ? 0.9f : 0;
        windZone.windPulseFrequency = windMain>0 ? 0.05f : 0;
    }

    void SetRandomTarget()
    {
        float randomRotation = Random.Range(-180f, 180f);
        int rotationDirection = Random.Range(0, 2); // 0 - по часовой стрелке, 1 - против часовой стрелки
        if (rotationDirection == 0)
        {
            targetAngle = transform.rotation.eulerAngles.y + randomRotation;
        }
        else
        {
            targetAngle = transform.rotation.eulerAngles.y - randomRotation;
        }
        targetWindStrength = Random.Range(maxWindStrength/2, maxWindStrength);
    }
}
