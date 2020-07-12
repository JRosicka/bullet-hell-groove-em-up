using System.Collections.Generic;

/// <summary>
/// Send a specific message to a Shot instance. Currently can only send a message to the Shot instance most recently scheduled to be
/// fired. 
/// </summary>
public class UpdateShotNoteAction : NoteAction {
    private string step;
    private readonly List<ConfigurationEvent> shotInstances;

    public UpdateShotNoteAction(int index, float triggerTime, List<ConfigurationEvent> shotInstances, string step) : base(index, triggerTime) {
        this.shotInstances = shotInstances;
        this.step = step;
    }
    public override void PerformAction() {
        shotInstances[Index].UpdateEvent(step);
    }
}
