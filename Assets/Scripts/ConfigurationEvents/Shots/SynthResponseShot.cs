using System;
using UnityEngine;

/// <summary>
/// The shot that goes "bya bya byaaa BA-BA"
/// Fires ring of static rotating shots
/// </summary>
public class SynthResponseShot : Shot {
    public BulletParticleSystem BSystem;
    public bool ClockWise;
    
    private PlayerController playerController;

    private new enum Values {
        None = Shot.Values.None,
        FireShot = Shot.Values.FireShot,
    }

    public override string[] GetValues() {
        return Enum.GetNames(typeof(Values));
    }

    private Values GetValueForString(string stringName) {
        return (Values)Enum.Parse(typeof(Values), stringName);
    }

    void Start() {
        playerController = FindObjectOfType<PlayerController>();
        // Fire the first barrage right away
        Shoot(transform, BSystem);
    }

    public override void Shoot(Transform spawner, BulletParticleSystem system) {
        if (!ClockWise) {
            BSystem.GetActiveParticleSystems().ForEach(e => e.transform.Rotate(180, 0, 0));
        }
        system.Shoot();
    }

    public override void UpdateEvent(string step) {
        Values action = GetValueForString(step);
    }
}
