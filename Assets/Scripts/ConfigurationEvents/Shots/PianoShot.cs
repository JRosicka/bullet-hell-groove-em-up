using System;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// The shot that goes "buh, beDAAAA"
/// Fires three rounds of projectiles that each aim at the player
/// </summary>
public class PianoShot : Shot {
    [Header("Spawns")]
    public Transform Spawner1;
    public Transform Spawner2;
    public Transform Spawner3;

    [Header("Particle Systems")]
    public BulletParticleSystem System1;
    public BulletParticleSystem System2;
    public BulletParticleSystem System3;

    private PlayerController playerController;

    private new enum Values {
        None = Shot.Values.None,
        FireShot = Shot.Values.FireShot,
        SecondShot = 100,
        ThirdShot = 101,
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
        Shoot(Spawner1, System1);
    }

    public override void Shoot(Transform spawner, BulletParticleSystem system) {
        Vector3 target = playerController.GetPlayerPosition();
        Vector3 origin = spawner.transform.position;

        spawner.rotation = CalculateRotation(origin, target);
        system.Shoot();
    }

    public override void UpdateEvent(string step) {
        Values action = GetValueForString(step);
        switch (action) {
            case Values.SecondShot:
                Shoot(Spawner2, System2);
                break;
            case Values.ThirdShot:
                Shoot(Spawner3, System3);
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    /// <summary>
    /// Figure out which way to face in order to aim at the target
    /// </summary>
    /// <param name="origin">Location of the shooter</param>
    /// <param name="target">Location of the target</param>
    private Quaternion CalculateRotation(Vector3 origin, Vector3 target) {
        float angleRad = Mathf.Atan2(target.y - origin.y, target.x - origin.x);
        float angleDeg = (180 / Mathf.PI) * angleRad;
        return Quaternion.Euler(0, 0, angleDeg);
    }
}
