using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// An event that we can schedule to perform via a PatternMeasure
/// </summary>
public abstract class ConfigurationEvent : MonoBehaviour {
    public enum Values {
        Unset = -1,
        None = 0,
    }

    public virtual string[] GetValues() {
        ArrayList list = new ArrayList(Enum.GetNames(typeof(Values)));
        list.RemoveAt(0);
        return (string[])list.ToArray();
    }

    public static Values GetBaseValueForString(string name) {
        return Enum.TryParse(name, out Values ret) ? ret : Values.Unset;
    }
    
    public abstract void UpdateEvent(string step);
}