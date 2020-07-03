using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction {
    public int Index;
    public float TriggerTime;
    public Shot Shot;

    // TODO: Some of these, like shot, we don't need for all children
    public BaseAction(int index, float triggerTime, Shot shot) {
        Index = index;
        TriggerTime = triggerTime;
        Shot = shot;
    }

    public abstract void PerformAction();
}