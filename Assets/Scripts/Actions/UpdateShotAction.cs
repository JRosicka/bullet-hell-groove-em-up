using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateShotAction : BaseAction {
    private string step;

    public UpdateShotAction(int index, float triggerTime, List<Shot> shotInstances, string step) : base(index, triggerTime, shotInstances) {
        this.step = step;
    }
    public override void PerformAction() {
        shotInstances[Index].UpdateShot(step);
    }
}
