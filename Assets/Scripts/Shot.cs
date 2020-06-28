using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: Move more things here once we know what we'll want for all shot types
public abstract class Shot : MonoBehaviour {
    public abstract void Shoot(Transform spawner, ParticleSystem system);
}
