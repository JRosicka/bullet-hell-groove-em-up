using System.Collections.Generic;

/// <summary>
/// A NoteAction is a thing that is triggered from a pattern. It is created upon initialization of the pattern and assigned a TriggerTime
/// value for when to trigger. 
/// </summary>
public abstract class NoteAction {
    protected readonly int Index;
    public readonly float TriggerTime;
    protected readonly List<Shot> ShotInstances;

    protected NoteAction(int index, float triggerTime, List<Shot> shotInstances) {
        Index = index;
        TriggerTime = triggerTime;
        ShotInstances = shotInstances;
    }

    /// <summary>
    /// Generic "Do the thing" method called when the TriggerTime expires
    /// </summary>
    public abstract void PerformAction();
}