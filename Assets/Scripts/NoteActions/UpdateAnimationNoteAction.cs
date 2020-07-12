
/// <summary>
/// Send a specific message to a Shot instance. Currently can only send a message to the Shot instance most recently scheduled to be
/// fired. 
/// </summary>
public class UpdateAnimationNoteAction : NoteAction {
    private string step;
    private AnimationEvent animationEvent;
    
    // TODO: We aren't using the shotIndex value here, and it is also probably wrong.
    public UpdateAnimationNoteAction(int index, float triggerTime, AnimationEvent animationEvent, string step) : base(index, triggerTime) {
        this.step = step;
        this.animationEvent = animationEvent;
    }
    public override void PerformAction() {
        animationEvent.UpdateEvent(step);
    }
}