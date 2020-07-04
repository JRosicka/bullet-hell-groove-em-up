using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Move more things here once we know what we'll want for all shot types
public abstract class Shot : MonoBehaviour {
    public enum Values {
        Unset = -1,
        None = 0,
        FireShot = 1
    }

    public virtual string[] GetValues() {
        ArrayList list = new ArrayList(Enum.GetNames(typeof(Values)));
        list.RemoveAt(0);
        return (string[])list.ToArray();
    }

    public static Values GetBaseValueForString(string name) {
        Values ret;
        if (Enum.TryParse<Values>(name, out ret))
            return ret;
        else
            return Values.Unset;
    }

    public abstract void Shoot(Transform spawner, ParticleSystem system);

    public abstract void UpdateShot(string step);
}