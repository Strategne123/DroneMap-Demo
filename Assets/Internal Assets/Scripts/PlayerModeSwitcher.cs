using UnityEngine;
using Invector.vCharacterController;

public class PlayerModeSwitcher : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [SerializeField] private GameObject body;
    [SerializeField] private GameObject pilot;
    [SerializeField] private GameObject pilotMagicTPV;

    private bool isAlternateMode = false;
    private bool bodyInitialActive;
    private bool thirdPersonControllerInitialState;
    private bool pilotRigidbodyInitiallyDynamic;
    private bool pilotColliderInitialState;
    private float initialMinDistance;
    private float initialMaxDistance;

    private vThirdPersonController thirdPersonController;
    private Rigidbody pilotRigidbody;
    private Collider pilotCollider;
    private TPV tpv;

    [Header("Testing")]
    [SerializeField] private bool testTogglePlayerMode;

    [Header("Defense Mode")]
    [SerializeField] private GameObject viarusHMD;

    private void Update()
    {
        if (testTogglePlayerMode)
        {
            TogglePlayerMode();
            testTogglePlayerMode = false;
        }
    }

    private void Start()
    {
        if (body != null)
            bodyInitialActive = body.activeSelf;

        if (pilot != null)
        {
            thirdPersonController = pilot.GetComponent<vThirdPersonController>();
            if (thirdPersonController != null)
                thirdPersonControllerInitialState = thirdPersonController.enabled;

            pilotRigidbody = pilot.GetComponent<Rigidbody>();
            if (pilotRigidbody != null)
                pilotRigidbodyInitiallyDynamic = !pilotRigidbody.isKinematic;

            pilotCollider = pilot.GetComponent<Collider>();
            if (pilotCollider != null)
                pilotColliderInitialState = pilotCollider.enabled;
        }

        if (pilotMagicTPV != null)
        {
            tpv = pilotMagicTPV.GetComponent<TPV>();
            if (tpv != null)
            {
                initialMinDistance = tpv.minDistanceBehindTarget;
                initialMaxDistance = tpv.maxDistanceBehindTarget;
            }
            if (viarusHMD == null)
            {
                Transform viarusTransform = pilotMagicTPV.transform.Find("ViarusHMD");
                if (viarusTransform != null)
                    viarusHMD = viarusTransform.gameObject;
                else
                    Debug.LogWarning("ViarusHMD не найден среди дочерних объектов pilotMagicTPV");
            }
        }
    }

    public void TogglePlayerMode()
    {
        if (!isAlternateMode)
        {
            if (body != null)
                body.SetActive(false);

            if (thirdPersonController != null)
                thirdPersonController.enabled = false;

            if (pilotRigidbody != null)
                pilotRigidbody.isKinematic = true;

            if (pilotCollider != null)
                pilotCollider.enabled = false;

            if (tpv != null)
            {
                tpv.minDistanceBehindTarget = 0f;
                tpv.maxDistanceBehindTarget = 0f;
            }

            isAlternateMode = true;
        }
        else
        {
            if (body != null)
                body.SetActive(bodyInitialActive);

            if (thirdPersonController != null)
                thirdPersonController.enabled = thirdPersonControllerInitialState;

            if (pilotRigidbody != null)
                pilotRigidbody.isKinematic = !pilotRigidbodyInitiallyDynamic;

            if (pilotCollider != null)
                pilotCollider.enabled = pilotColliderInitialState;

            if (tpv != null)
            {
                tpv.minDistanceBehindTarget = initialMinDistance;
                tpv.maxDistanceBehindTarget = initialMaxDistance;
            }

            isAlternateMode = false;
        }
    }

    public void MoveToDefensePoint(Transform targetPoint)
    {
        if (viarusHMD == null)
        {
            viarusHMD = GameObject.Find("ViarusHMD");
            if (viarusHMD == null)
            {
                Debug.LogWarning("ViarusHMD не найден при перемещении");
                return;
            }
        }
        if (targetPoint != null)
        {
            viarusHMD.transform.position = targetPoint.position;
            viarusHMD.transform.rotation = targetPoint.rotation;
        }
    }
}
