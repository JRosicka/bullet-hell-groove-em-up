using UnityEngine;

// TODO: Move more things here once we know what we'll want for all shot types
/// <summary>
/// A Shot is a ConfigurationEvent that can be spawned into the scene to make bullets appear and generally look pretty and stuff
/// </summary>
public abstract class Shot : Pattern {    // TODO blarg we don't need this anymore probably
    public abstract void Shoot(Transform spawner, BulletParticleSystem system);
}