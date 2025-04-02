using System;
using UnityEngine;
//using ViarusTask;

public class LevelOfGlitches : MonoBehaviour
{
    private float _level;
    [HideInInspector] public bool isOn = false;
    public VirtualDrone drone { get; set; }

    [Range(0f, 1f)]
    public float LevelSlider;

    [SerializeField] private SphereCollider signalRadius;
    private MultiPassShaderController multiPassShaderController;
    [SerializeField] private Transform UICamera;


    [SerializeField] private Material whiteNoise;
    private float whiteNoiseMax = 0.07f;

    //private RGBShiftEffect rGBShiftEffect;
    [SerializeField] private Material rGBShiftEffect;
    private float rGBShiftEffectAmount = 0.02f;
    private float rGBShiftEffectAmountMax = 0.125f;
    private float rGBShiftEffectSpeed = 7f;
    private float rGBShiftEffectSpeedMax = 15f;

    //private BadTVEffect badTVEffect;
    [SerializeField] private Material badTVEffect;
    private float badTVEffectThickDistort = 13f;//18f;//6f;
    private float badTVEffectThickDistortMax = 40f;
    private float badTVEffectFineDistort = 7f;//7.5f//5f;
    private float badTVEffectFineDistortMax = 100f;

    //private ScanlinesEffect scanlinesEffect;
    [SerializeField] private Material scanlinesEffect;
    private float scanlinesEffectStrength = 1.25f;
    private float scanlinesEffectStrengthMax = 1.75f;

    //private RippleEffect rippleEffect;
    [SerializeField] private Material rippleEffect;
    private float rippleEffectStrength = 0.004f;
    private float rippleEffectStrengthMax = 0.015f;
    private float rippleEffectAmount = 7f;
    private float rippleEffectSpeed = 10f;


    private float transitionToMaxValues = 0.8f;
    private float transitionFromZeroValues = 0.07f;

    private float ScanLines;
    private float RGB;
    private float BadTV;
    private float WhiteNoise;
    private float Ripple;

    public float Level
    {
        get { return _level; }
        set
        {
            try
            {
                var mas = drone.GetBoolVariables();
                ScanLines = Convert.ToSingle(mas[eBoolVariables.ScanLines].isOn && mas[eBoolVariables.Glitches].isOn);
                RGB = Convert.ToSingle(mas[eBoolVariables.RGB].isOn && mas[eBoolVariables.Glitches].isOn);
                BadTV = Convert.ToSingle(mas[eBoolVariables.BadTV].isOn && mas[eBoolVariables.Glitches].isOn);
                WhiteNoise = Convert.ToSingle(mas[eBoolVariables.WhiteNoise].isOn && mas[eBoolVariables.Glitches].isOn);
                Ripple = Convert.ToSingle(mas[eBoolVariables.Ripple].isOn && mas[eBoolVariables.Glitches].isOn);
            }
            catch { }

            scanlinesEffect.SetFloat("_Strength", value * scanlinesEffectStrength * ScanLines);
            //print($" Level value: {value}");
            value -= transitionFromZeroValues;
            if (value > 1) value = 1;
            else if (value < 0) value = 0;
            //print($" Level Level: {value}");
            if (_level == 0 && value > 0)
            {
                if(multiPassShaderController ==  null)
                {
                    multiPassShaderController = FindObjectOfType<MultiPassShaderController>();
                }
                multiPassShaderController.IsOn = true;
                //rGBShiftEffect.enabled = true;
                //badTVEffect.enabled = true;
                //scanlinesEffect.enabled = true;
                //rippleEffect.enabled = true;
            }
            else if (value < 0 && _level > 0)
            {
                if (multiPassShaderController == null)
                {
                    multiPassShaderController = FindObjectOfType<MultiPassShaderController>();
                }
                multiPassShaderController.IsOn = false;
                //rGBShiftEffect.enabled = false;
                //badTVEffect.enabled = false;
                //scanlinesEffect.enabled = false;
                //rippleEffect.enabled = false;
            }
            try
            {
                multiPassShaderController.IsOn = multiPassShaderController.IsOn && drone.IsGlitchEnable();
                rippleEffect.SetFloat("_Strength", value * rippleEffectStrength * (multiPassShaderController.IsOn ? 1 : 0));
            }
            catch { }
            if (value < transitionToMaxValues)
            {
                whiteNoise.SetFloat("_Power", value * whiteNoiseMax * WhiteNoise);

                //rGBShiftEffect.amount = value * rGBShiftEffectAmount;
                //rGBShiftEffect.speed = value * rGBShiftEffectSpeed;
                rGBShiftEffect.SetFloat("_Amount", value * rGBShiftEffectAmount * RGB);
                rGBShiftEffect.SetFloat("_Speed", value * rGBShiftEffectSpeed * RGB);

                //badTVEffect.thickDistort = value * badTVEffectThickDistort;
                //badTVEffect.fineDistort = value * badTVEffectFineDistort;
                badTVEffect.SetFloat("_ThickDistort", value * badTVEffectThickDistort * BadTV);
                badTVEffect.SetFloat("_FineDistort", value * badTVEffectFineDistort * BadTV);


                //rippleEffect.strength = value * rippleEffectStrength;
                rippleEffect.SetFloat("_Strength", value * rippleEffectStrength * Ripple);




                //rippleEffect.amount = (int)(value * rippleEffectAmount);
                //rippleEffect.speed = value * rippleEffectSpeed;
            }
            else
            {
                float newValue = (value - transitionToMaxValues) * (1 / (1 - transitionToMaxValues - transitionFromZeroValues));

                whiteNoise.SetFloat("_Power", Mathf.Lerp(whiteNoiseMax, 1, newValue) * WhiteNoise);

                //rGBShiftEffect.amount = Mathf.Lerp(rGBShiftEffectAmount, rGBShiftEffectAmountMax, newValue);
                rGBShiftEffect.SetFloat("_Amount", Mathf.Lerp(rGBShiftEffectAmount, rGBShiftEffectAmountMax, newValue) * RGB);
                rGBShiftEffect.SetFloat("_Speed", Mathf.Lerp(rGBShiftEffectSpeed, rGBShiftEffectSpeedMax, newValue) * RGB);
                badTVEffect.SetFloat("_ThickDistort", Mathf.Lerp(badTVEffectThickDistort, badTVEffectThickDistortMax, newValue) * BadTV);
                badTVEffect.SetFloat("_FineDistort", Mathf.Lerp(badTVEffectFineDistort, badTVEffectFineDistortMax, newValue) * BadTV);
                scanlinesEffect.SetFloat("_Strength", Mathf.Lerp(scanlinesEffectStrength, scanlinesEffectStrengthMax, newValue) * ScanLines);
                rippleEffect.SetFloat("_Strength", Mathf.Lerp(rippleEffectStrength, rippleEffectStrengthMax, newValue) * Ripple);
            }

            _level = value;
        }
    }

    

    public void SetSignalRadiusToPilot(IPilot pilot)
    {
        signalRadius.transform.SetParent(pilot.GetViarusTransform());
        signalRadius.transform.localPosition = Vector3.zero;
    }

    public void SetSignalRadiusToDrone(VirtualDrone drone)
    {
        signalRadius.transform.SetParent(null);
        signalRadius.transform.position = drone.transform.position;
    }

    public void Init()
    {
        if (drone.isOwned)
        {
            //rGBShiftEffect = GetComponent<RGBShiftEffect>();
            //rGBShiftEffectAmount = rGBShiftEffect.amount;
            //rGBShiftEffectSpeed = rGBShiftEffect.speed;
            //badTVEffect = GetComponent<BadTVEffect>();
            //badTVEffectThickDistort = badTVEffect.thickDistort;
            //badTVEffectFineDistort = badTVEffect.fineDistort;

            /*        scanlinesEffect = GetComponent<ScanlinesEffect>();
                    scanlinesEffectStrength = scanlinesEffect.strength;*/

            //rippleEffect = GetComponent<RippleEffect>();
            //rippleEffectStrength = rippleEffect.strength;
            /*        rippleEffectAmount = rippleEffect.amount;
                    rippleEffectSpeed = rippleEffect.speed;*/
            multiPassShaderController = FindObjectOfType<MultiPassShaderController>();
            scanlinesEffect.SetFloat("_Speed", 12);
            scanlinesEffect.SetFloat("_Noise", 0.33f);

            Level = 0;
            LevelSlider = 0;
            signalRadius = GetComponentInChildren<SphereCollider>();
            UICamera.SetParent(null);
        }
        else
        {
            UICamera.gameObject.SetActive(false);
        }
    }


    public void FixedUpdater()
    {
        //if (Level != LevelSlider)
        //{
        //    Level = LevelSlider;
        //}
        if (!isOn)
        {
            Level = 0;
            return;
        }
        try
        {
            Level = Mathf.Pow(Vector3.Distance(drone.transform.position, signalRadius.transform.position) / signalRadius.radius, Mathf.Exp(0.75f));
        }
        catch { /*Debug.Log("Can't find signalRadius!!!");*/ }
        //Level = (drone.droneInput.GetThrust() + 1) / 2f;
    }


    // Чтобы избежать лишних изменений материалов в коммитах
    private void OnApplicationQuit()
    {
        Level = 0;
    }
}
