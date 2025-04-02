using UnityEngine;

public class TPV : MonoBehaviour
{
    public Transform objectToFollow;
    public float minDistanceBehindTarget/* = 2f*/;
    public float maxDistanceBehindTarget/* = 5f*/;
    public float rotationSpeed = 5f;
    public float elevation;
    private Quaternion targetRotation;
   

    private void Update()
    {
        if (objectToFollow == null)
        {
            return;
        }
        float currentSpeed = objectToFollow.GetComponent<Rigidbody>().velocity.magnitude / 1;
        float distance = Mathf.Clamp(currentSpeed, minDistanceBehindTarget, maxDistanceBehindTarget);

        Vector3 targetPosition = objectToFollow.position - objectToFollow.forward * distance;
        targetPosition.y = objectToFollow.position.y + elevation;
        transform.position = targetPosition;

        //Vector3 lookAtPos = new Vector3(objectToFollow.position.x, objectToFollow.position.y, objectToFollow.position.z);
        //transform.LookAt(lookAtPos);

        /*targetRotation = Quaternion.Slerp(targetRotation, objectToFollow.rotation * Quaternion.AngleAxis(-10, Vector3.right), Time.deltaTime * rotationSpeed);
        transform.rotation = targetRotation;*/
    }
}