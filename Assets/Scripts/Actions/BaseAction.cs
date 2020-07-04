using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction {
    public int Index;
    public float TriggerTime;
    protected List<Shot> shotInstances;

    public BaseAction(int index, float triggerTime, List<Shot> shotInstances) {
        Index = index;
        TriggerTime = triggerTime;
        this.shotInstances = shotInstances;
    }

    public abstract void PerformAction();
}