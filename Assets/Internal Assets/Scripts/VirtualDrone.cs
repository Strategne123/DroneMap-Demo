using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class VirtualDrone : MonoBehaviour
{
    public bool isOwned = false;
    public bool isServer = false;
    private Level _level;
    public Level level
    {
        get { return _level; }
        set
        {
            previuosLevel = _level;
            _level = value;
        }
    }
    public Level previuosLevel { get; private set; }


    public virtual void SetOutline(bool outlineEnabled) { }

    public virtual void SetOutlineColor(Color color) { }

    public virtual bool IsJoyEnable() { return false; }

    public virtual bool IsGlitchEnable() { return false; }

    public virtual void ExitToScene(string sceneName) { }

    public virtual int GetCountCarousels() { return 0; }

    public virtual int GetCountBoolVariables() { return 0; }

    public virtual Dictionary<eTypeSettings, Carousel> GetCarousels() { return null; }

    public virtual Dictionary<eBoolVariables, Toggle> GetBoolVariables() { return null; }

    public virtual void SelectAmmo(eAmmo ammoType) { }

    public virtual void SetControllerParameter(eTypeSettings typeSettings, int value) { }

    public virtual void SetBatteryLifeTime(int lifeTime) { }

    public virtual void SetDroneBody(int index) { }

    public virtual bool IsExp() { return false; }

}
