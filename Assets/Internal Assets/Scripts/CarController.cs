using UnityEngine;
using Mirror;

public class CarController : NetworkBehaviour
{
    [Header("Настройки автомобиля")]
    [Tooltip("Максимальная скорость (км/ч)")]
    public float maxSpeedKMH = 100f;

    [Tooltip("Ускорение (м/с²)")]
    public float accelerationMS = 5f;

    [Tooltip("Сила торможения")]
    public float brakeForce = 3000f;

    [Tooltip("Максимальный угол поворота колес (°)")]
    public float maxSteerAngle = 30f;

    [Tooltip("Смещение передней точки от центра автомобиля (м)")]
    public float frontOffset = 1f;

    [Header("Коллайдеры колес")]
    [Tooltip("Переднее левое колесо")]
    public WheelCollider wheelFrontLeft;

    [Tooltip("Переднее правое колесо")]
    public WheelCollider wheelFrontRight;

    [Tooltip("Заднее левое колесо")]
    public WheelCollider wheelRearLeft;

    [Tooltip("Заднее правое колесо")]
    public WheelCollider wheelRearRight;

    [Header("Настройки центра масс")]
    [Tooltip("Смещение центра массы автомобиля")]
    public Vector3 centerOfMassOffset = new Vector3(0f, -0.5f, 0f);

    private Rigidbody rb;

    [HideInInspector]
    public float currentSpeedKMH;

    private float verticalInput;
    private float horizontalInput;

    [HideInInspector]
    public float PreviousSteering = 0f;

    private MachineDamage machineDamage;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMassOffset;

        machineDamage = GetComponent<MachineDamage>();
    }

    [Server]
    public void SetInput(float vertical, float horizontal)
    {
        verticalInput = Mathf.Clamp(vertical, -1f, 1f);
        horizontalInput = Mathf.Clamp(horizontal, -1f, 1f);
    }

    [ServerCallback]
    void FixedUpdate()
    {
        if (!isServer)
            return;

        UpdatePhysics();
        // UpdateWheelVisuals(); 
    }

    private void UpdatePhysics()
    {
        if (machineDamage != null && machineDamage.isExp)
        {
            ApplyBrakes();
            return;
        }

        currentSpeedKMH = rb.velocity.magnitude * 3.6f;

        float motorTorque = 0f;
        float brakeTorque = 0f;

        if (verticalInput > 0f)
        {
            if (currentSpeedKMH < maxSpeedKMH)
                motorTorque = verticalInput * accelerationMS * rb.mass;
        }
        else if (verticalInput < 0f)
        {
            brakeTorque = brakeForce * -verticalInput;
        }
        else
        {
            brakeTorque = brakeForce * 0.1f;
        }

        float steeringAngle = horizontalInput * maxSteerAngle;

        wheelFrontLeft.steerAngle = steeringAngle;
        wheelFrontRight.steerAngle = steeringAngle;

        wheelFrontLeft.motorTorque = motorTorque;
        wheelFrontRight.motorTorque = motorTorque;

        wheelFrontLeft.brakeTorque = brakeTorque;
        wheelFrontRight.brakeTorque = brakeTorque;
        wheelRearLeft.brakeTorque = brakeTorque;
        wheelRearRight.brakeTorque = brakeTorque;

        wheelRearLeft.motorTorque = 0f;
        wheelRearRight.motorTorque = 0f;

    }

    private void ApplyBrakes()
    {
        float steeringAngle = 0f;
        float motorTorque = 0f;
        float brakeTorque = brakeForce * 1.0f; // Максимальное торможение

        wheelFrontLeft.steerAngle = steeringAngle;
        wheelFrontRight.steerAngle = steeringAngle;

        wheelFrontLeft.motorTorque = motorTorque;
        wheelFrontRight.motorTorque = motorTorque;

        wheelFrontLeft.brakeTorque = brakeTorque;
        wheelFrontRight.brakeTorque = brakeTorque;
        wheelRearLeft.brakeTorque = brakeTorque;
        wheelRearRight.brakeTorque = brakeTorque;

        wheelRearLeft.motorTorque = 0f;
        wheelRearRight.motorTorque = 0f;

        Debug.Log($"[{name}] Brakes applied.");
    }

    // Метод для обновления визуального состояния колес (если требуется)
    private void UpdateWheelVisuals()
    {
        // Ваш код
    }

    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        Gizmos.color = Color.red;
        Vector3 frontPoint = transform.position + transform.forward * frontOffset;
        Gizmos.DrawSphere(frontPoint, 0.1f);

        if (rb != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(transform.position + rb.centerOfMass, 0.1f);
        }
#endif
    }
}
