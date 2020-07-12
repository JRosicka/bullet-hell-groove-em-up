using System.Collections.Generic;

/// <summary>
/// A NoteAction is a thing that is triggered from a pattern. It is created upon initialization of the pattern and assigned a TriggerTime
/// value for when to trigger. 
/// </summary>
public abstract class NoteAction {
    protected readonly int Index;
    public readonly float TriggerTime;

    protected NoteAction(int index, float triggerTime) {
        Index = index;
        TriggerTime = triggerTime;
    }

    /// <summary>
    /// Generic "Do the thing" method called when the TriggerTime expires
    /// </summary>
    public abstract void PerformAction();
}