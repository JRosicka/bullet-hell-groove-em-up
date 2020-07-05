using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PianoShot : Shot {
    [Header("Spawns")]
    public Transform Spawner1;
    public Transform Spawner2;
    public Transform Spawner3;

    [Header("Particle Systems")]
    public ParticleSystem System1;
    public ParticleSystem System2;
    public ParticleSystem System3;

    private PlayerController playerController;

    public new enum Values {
        None = Shot.Values.None,
        FireShot = Shot.Values.FireShot,
        SecondShot = 100,
        ThirdShot = 101,
    }

    public override string[] GetValues() {
        return Enum.GetNames(typeof(Values));
    }

    private Values GetValueForString(string name) {
        return (Values)Enum.Parse(typeof(Values), name);
    }

    void Start() {
        playerController = FindObjectOfType<PlayerController>();
        // Fire the first barrage right away
        Shoot(Spawner1, System1);
    }

    public override void Shoot(Transform spawner, ParticleSystem system) {
        Vector3 target = playerController.GetPlayerPosition();
        Vector3 origin = spawner.transform.position;

        spawner.rotation = CalculateRotation(origin, target);
        system.Play();
    }

    public override void UpdateShot(string step) {
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

    private Quaternion CalculateRotation(Vector3 origin, Vector3 target) {
        float AngleRad = Mathf.Atan2(target.y - origin.y, target.x - origin.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        return Quaternion.Euler(0, 0, AngleDeg);
    }
}
