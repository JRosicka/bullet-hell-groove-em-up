using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateShotAction : BaseAction {
    public int Step;
    public List<Shot> ShotInstances;

    // TODO: I think we don't need both the index and the shot 
    public UpdateShotAction(int index, float triggerTime, Shot shot, List<Shot> shotInstances, int step) : base(index, triggerTime, shot) {
        ShotInstances = shotInstances;
        Step = step;
    }
    public override void PerformAction() {
        ShotInstances[Index].UpdateShot(Step);
    }
}
