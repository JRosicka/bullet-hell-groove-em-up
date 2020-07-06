using System;
using System.Collections;
using UnityEngine;

// TODO: Move more things here once we know what we'll want for all shot types
/// <summary>
/// A Shot can be spawned into the scene to make bullets appear and generally look pretty and stuff
/// </summary>
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
        return Enum.TryParse(name, out Values ret) ? ret : Values.Unset;
    }

    public abstract void Shoot(Transform spawner, BulletParticleSystem system);

    public abstract void UpdateShot(string step);
}