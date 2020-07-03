using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShotAction : BaseAction
{
    private Transform spawner;
    private Shot shotPrefab;

    public FireShotAction(int index, float triggerTime, List<Shot> shotInstances, Shot shotPrefab, Transform spawner) : base(index, triggerTime, shotInstances) {
        this.spawner = spawner;
        this.shotPrefab = shotPrefab;
    }
    public override void PerformAction() {
        // Shoot your shot.Shot, my dude
        Shot shotInstance = Object.Instantiate(this.shotPrefab, spawner) as Shot;
        shotInstances.Add(shotInstance);
    }
}
