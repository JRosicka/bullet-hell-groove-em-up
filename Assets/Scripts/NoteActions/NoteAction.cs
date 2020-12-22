using System;

/// <summary>
/// A NoteAction is a thing that is triggered from a pattern. It is created upon initialization of the pattern and assigned a TriggerTime
/// value for when to trigger. 
/// </summary>
public class NoteAction {
    public readonly float TriggerTime;
    private PatternAction patternAction;
    private Func<Pattern> patternInstanceGetter;
    private string serializedParameter;

    public NoteAction(float triggerTime, PatternAction patternAction, Func<Pattern> patternInstanceGetter, string serializedParameter) {
        TriggerTime = triggerTime;
        this.patternAction = patternAction;
        this.patternInstanceGetter = patternInstanceGetter;
        this.serializedParameter = serializedParameter;
    }

    /// <summary>
    /// Generic "Do the thing" method called when the TriggerTime expires
    /// </summary>
    public void PerformAction() {
        Pattern patternInstance = patternInstanceGetter.Invoke();
        patternInstance.InvokePatternAction(patternAction.ID, serializedParameter);
    }
}