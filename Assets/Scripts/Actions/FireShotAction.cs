using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireShotAction : BaseAction
{
    public Transform Spawner;
    public Shot ShotInstance;
    public List<Shot> ShotInstances;
    public FireShotAction(int index, float triggerTime, Shot shot, List<Shot> shotInstances, Transform spawner) : base(index, triggerTime, shot) {
        Spawner = spawner;
        ShotInstances = shotInstances;
    }
    public override void PerformAction() {
        // Shoot your shot.Shot, my dude
        ShotInstance = Object.Instantiate(Shot, Spawner) as Shot;
        ShotInstances.Add(ShotInstance);
    }
}
