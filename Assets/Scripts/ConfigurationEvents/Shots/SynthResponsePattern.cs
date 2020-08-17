using System;
using UnityEngine;

/// <summary>
/// The shot that goes "bya bya byaaa BA-BA"
/// Fires ring of static rotating shots
/// </summary>
public class SynthResponsePattern : Pattern {
    public BulletParticleSystem BSystem;
    public bool ClockWise;
    
    // ReSharper disable once UnusedMember.Global
    public void FireShot() {
        Shoot(transform, BSystem);
    }

    private void Shoot(Transform spawner, BulletParticleSystem system) {
        // Shot shotInstance = Object.Instantiate(shotPrefab, spawner.position, spawner.rotation, PlayerController.Instance.ShotBucket);

        if (!ClockWise) {
            BSystem.GetActiveParticleSystems().ForEach(e => e.transform.Rotate(180, 0, 0));
        }
        system.Shoot();
    }
}
