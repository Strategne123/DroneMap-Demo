using UnityEngine;
using ArcadeVehicleController;
using System.Collections.Generic;

public class CarAIController : MonoBehaviour
{
    public enum CarState { Drive, Exploded, Waiting, NotDetected }

    public CarState currentState = CarState.Drive;

    [Header("Sensors")]
    public Transform frontCenterSensor;
    public Transform frontRightNearSensor;
    public Transform frontRightFarSensor;
    public Transform rearRightNearSensor;
    public Transform rearRightFarSensor;
    public Transform frontHorizontalSensor;

    // New sensors for overtaking
    public Transform frontLeftNearSensor;
    public Transform frontLeftFarSensor;
    public Transform rearLeftNearSensor;
    public Transform rearLeftFarSensor;
    public Transform overtakeCheckSensor;

    [Header("Car Settings")]
    public float maxSteerAngle = 30f;
    public float sensorLength = 1f;
    public float horizontalSensorLength = 10f;
    public float overtakeCheckSensorLength = 5f;
    public float sensorOffsetMultiplier = 0.1f;
    public float turnSpeed = 30f;
    public float desiredSpeed = 50f;
    public float turnBrakeTorque = 1500f;
    public float frontCarBrakeTorque = 3000f;
    public bool useLeftSensors = true;

    [Header("Debug Info")]
    public float currentSpeed;
    public CarState carInFrontState = CarState.NotDetected;
    public bool isOvertaking = false;

    private Vehicle vehicle;
    private float logTimer = 0f;
    private bool overtaking = false;
    private bool overtakeCheckPassed = false;
    private MachineDamage machineDamage;

    private List<Transform> rightSensors;
    private List<Transform> leftSensors;
    private bool sawCarDuringOvertake = false;

    private void Start()
    {
        vehicle = GetComponent<Vehicle>();
        machineDamage = GetComponent<MachineDamage>();

        rightSensors = new List<Transform> { frontRightNearSensor, frontRightFarSensor, rearRightNearSensor, rearRightFarSensor };
        leftSensors = new List<Transform> { frontLeftNearSensor, frontLeftFarSensor, rearLeftNearSensor, rearLeftFarSensor };
    }

    private void FixedUpdate()
    {
        if (machineDamage.isExp)
            return;

        currentSpeed = vehicle.Velocity.magnitude * 3.6f;
        Vector3 offset = transform.forward * currentSpeed * sensorOffsetMultiplier;

        bool[] rightOnRoad = CheckSensors(rightSensors, offset);
        bool[] leftOnRoad = CheckSensors(leftSensors, offset);

        float steer = 0;
        float targetSpeed = desiredSpeed;

        if (overtaking)
        {
            isOvertaking = true;
            HandleOvertaking(useLeftSensors, !useLeftSensors ? rightOnRoad : leftOnRoad, ref steer, ref targetSpeed);
        }
        else
        {
            isOvertaking = false;
            HandleDriving(useLeftSensors, !useLeftSensors ? rightOnRoad : leftOnRoad, ref steer, ref targetSpeed);
        }

        if (CheckForCarInFront(out carInFrontState))
        {
            if (carInFrontState == CarState.Exploded)
            {
                StartOvertaking();
            }
            else
            {
                ApplySteer(steer);
                ApplyBrakeAndThrottle(frontCarBrakeTorque, 0f);
            }
        }
        else
        {
            carInFrontState = CarState.NotDetected;
            if (overtaking)
            {
                ContinueOvertaking(overtakeCheckSensor.position + offset);
            }
            else
            {
                ApplySteer(steer);
                ControlSpeed(targetSpeed, currentSpeed);
            }
        }

        /*DebugDrawSensors(rightSensors, rightOnRoad);
        DebugDrawSensors(leftSensors, leftOnRoad);
        DebugDrawHorizontalRay(frontHorizontalSensor.position, horizontalSensorLength, carInFrontState);
        DebugDrawOvertakeCheckRay(overtakeCheckSensor.position + offset, overtakeCheckPassed);*/

        logTimer += Time.deltaTime;
        if (logTimer > 1f)
        {
            logTimer = 0f;
        }
    }

    private bool[] CheckSensors(List<Transform> sensors, Vector3 offset)
    {
        bool[] onRoad = new bool[sensors.Count];
        for (int i = 0; i < sensors.Count; i++)
        {
            onRoad[i] = IsOnRoad(sensors[i].position + offset);
        }
        return onRoad;
    }

    private bool IsOnRoad(Vector3 sensorPosition)
    {
        return Physics.Raycast(sensorPosition, Vector3.down, out RaycastHit hit, sensorLength) && hit.collider.CompareTag("Road");
    }

    private bool CheckForCarInFront(out CarState carInFrontState)
    {
        carInFrontState = CarState.NotDetected;
        if (Physics.Raycast(frontHorizontalSensor.position, transform.forward, out RaycastHit hit, horizontalSensorLength) && hit.collider.CompareTag("Car"))
        {
            CarAIController otherCar = hit.collider.GetComponent<CarAIController>();
            if (otherCar != null)
            {
                carInFrontState = otherCar.currentState;
                return true;
            }
        }
        return false;
    }

    private void HandleOvertaking(bool useLeft, bool[] onRoad, ref float steer, ref float targetSpeed)
    {
        if (!IsOnRoad(frontCenterSensor.position + transform.forward * currentSpeed * sensorOffsetMultiplier))
        {
            steer = useLeft ? -maxSteerAngle : maxSteerAngle;
            targetSpeed = turnSpeed;
        }
        else if (onRoad[0] && !onRoad[1])
        {
            steer = 0;
        }
        else if (onRoad[0] && onRoad[1])
        {
            steer = useLeft ? maxSteerAngle : -maxSteerAngle;
            targetSpeed = turnSpeed;
        }
        else if (!onRoad[0] && !onRoad[1])
        {
            steer = useLeft ? -maxSteerAngle : maxSteerAngle;
            targetSpeed = turnSpeed;
        }
    }

    private void HandleDriving(bool useLeft, bool[] onRoad, ref float steer, ref float targetSpeed)
    {
        if (!IsOnRoad(frontCenterSensor.position + transform.forward * currentSpeed * sensorOffsetMultiplier))
        {
            steer = useLeft ? maxSteerAngle : -maxSteerAngle;
            targetSpeed = turnSpeed;
        }
        else if (onRoad[0] && !onRoad[1])
        {
            steer = 0;
        }
        else if (onRoad[0] && onRoad[1])
        {
            steer = useLeft ? -maxSteerAngle : maxSteerAngle;
            targetSpeed = turnSpeed;
        }
        else if (!onRoad[0] && !onRoad[1])
        {
            steer = useLeft ? maxSteerAngle : -maxSteerAngle;
            targetSpeed = turnSpeed;
        }
    }

    private void StartOvertaking()
    {
        overtaking = true;
        overtakeCheckPassed = false;
        useLeftSensors = !useLeftSensors;
        ApplySteer(-maxSteerAngle);
        ControlSpeed(turnSpeed, currentSpeed);
    }


    private void ContinueOvertaking(Vector3 overtakeCheckSensorPos)
    {
        RaycastHit hit;
        bool seen = Physics.Raycast(overtakeCheckSensorPos, transform.right, out hit, overtakeCheckSensorLength);

        if (seen && hit.collider.CompareTag("Car"))
        {
            sawCarDuringOvertake = true;
        }

        if (sawCarDuringOvertake && (!seen || !hit.collider.CompareTag("Car")))
        {
            overtaking = false;
            sawCarDuringOvertake = false;
            ApplySteer(maxSteerAngle);
            ControlSpeed(desiredSpeed, currentSpeed);
        }
    }

    private void ApplySteer(float steerAngle)
    {
        vehicle.SetSteerInput(steerAngle / maxSteerAngle);
    }

    private void ControlSpeed(float targetSpeed, float currentSpeedKmh)
    {
        float brakeTorque = 0f;
        float motorTorque = 1f;

        if (currentSpeedKmh > targetSpeed)
        {
            brakeTorque = (currentSpeedKmh - targetSpeed) * 0.1f;
            motorTorque = 0f;
        }
        else if (currentSpeedKmh < targetSpeed)
        {
            brakeTorque = 0f;
            motorTorque = 1f;
        }
        else
        {
            brakeTorque = 0f;
            motorTorque = 0f;
        }

        ApplyBrakeAndThrottle(brakeTorque, motorTorque);
    }

    private void ApplyBrakeAndThrottle(float brakeTorque, float motorTorque)
    {
        vehicle.SetAccelerateInput(motorTorque - brakeTorque);
    }
    /*
    private void DebugDrawRay(Vector3 sensorPosition, bool onRoad)
    {
        Color rayColor = onRoad ? Color.green : Color.red;
        Debug.DrawRay(sensorPosition, Vector3.down * sensorLength, rayColor);
    }

    private void DebugDrawHorizontalRay(Vector3 sensorPosition, float length, CarState carInFrontState)
    {
        Color rayColor = carInFrontState switch
        {
            CarState.Drive => Color.blue,
            CarState.Waiting => Color.yellow,
            CarState.Exploded => Color.red,
            _ => Color.green,
        };
        Debug.DrawRay(sensorPosition, transform.forward * length, rayColor);
    }

    private void DebugDrawOvertakeCheckRay(Vector3 sensorPosition, bool checkPassed)
    {
        Color rayColor = checkPassed ? Color.yellow : Color.green;
        Debug.DrawRay(sensorPosition, transform.right * overtakeCheckSensorLength, rayColor);
    }*/
}
