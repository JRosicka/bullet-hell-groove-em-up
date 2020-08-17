/// <summary>
/// A NoteAction is a thing that is triggered from a pattern. It is created upon initialization of the pattern and assigned a TriggerTime
/// value for when to trigger. 
/// </summary>
public class NoteAction {
    public readonly float TriggerTime;
    private Pattern.PatternAction patternAction;
    private Pattern patternInstance;

    public NoteAction(float triggerTime, Pattern.PatternAction patternAction, Pattern patternInstance) {
        TriggerTime = triggerTime;
        this.patternAction = patternAction;
        this.patternInstance = patternInstance;
    }

    /// <summary>
    /// Generic "Do the thing" method called when the TriggerTime expires
    /// </summary>
    public void PerformAction() {
        patternInstance.InvokePatternAction(patternAction.ID);
    }
}