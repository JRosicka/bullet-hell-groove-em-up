using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PianoShot : Shot {
    [Header("Spawns")]
    public Transform Spawner1;
    public Transform Spawner2;
    public Transform Spawner3;

    [Header("Particle Systems")]
    public ParticleSystem System1;
    public ParticleSystem System2;
    public ParticleSystem System3;

    [Header("Shot Times")]
    public float ShotTime1;
    public float ShotTime2;
    public float ShotTime3;

    private bool shot1Fired;
    private bool shot2Fired;
    private bool shot3Fired;

    private PlayerController playerController;
    private float timeElapsed = 0;

    void Start() {
        playerController = FindObjectOfType<PlayerController>();
    }

    void Update() {
        timeElapsed += Time.deltaTime;

        if (shot3Fired)
            return;
        if (timeElapsed >= ShotTime3) {
            shot3Fired = true;
            Shoot(Spawner3, System3);
        }

        if (shot2Fired)
            return;
        if (timeElapsed >= ShotTime2) {
            shot2Fired = true;
            Shoot(Spawner2, System2);
        }

        if (shot1Fired)
            return;
        if (timeElapsed >= ShotTime1) {
            shot1Fired = true;
            Shoot(Spawner1, System1);
        }
    }

    public override void Shoot(Transform spawner, ParticleSystem system) {
        Vector3 target = playerController.GetPlayerPosition();
        Vector3 origin = spawner.transform.position;

        spawner.rotation = CalculateRotation(origin, target);
        system.Play();
    }

    private Quaternion CalculateRotation(Vector3 origin, Vector3 target) {
        float AngleRad = Mathf.Atan2(target.y - origin.y, target.x - origin.x);
        float AngleDeg = (180 / Mathf.PI) * AngleRad;
        return Quaternion.Euler(0, 0, AngleDeg);
    }
}
