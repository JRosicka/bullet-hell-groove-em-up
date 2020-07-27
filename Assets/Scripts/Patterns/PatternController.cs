using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Parses through a PatternConfiguration and generates NoteActions based on the configured timings and action types.
/// NoteActions and instantiated and scheduled upon PatternController initialization. 
/// </summary>
public class PatternController : MonoBehaviour {
    private const int ACTIONS_PER_MEASURE = 32;

    public Transform Agent;
    public PatternConfiguration Pattern;

    private TimingController timingController;
    private float timeElapsed;

    // The original list of configured actions configured at start
    private List<NoteAction> configuredActions;
    // An updated list originally copied from configuredShots
    private List<NoteAction> queuedActions;
    // Shot instances are added here when they spawn. We pass this to UpdateNoteActions so that they can access the Shot instances. 
    private List<ConfigurationEvent> eventInstances = new List<ConfigurationEvent>();

    private void Start() {
        timingController = FindObjectOfType<TimingController>();
        configuredActions = ScheduleNoteActions();
        queuedActions = new List<NoteAction>(configuredActions);
    }

    /// <summary>
    /// Parses through a PatternConfiguration and generates NoteActions based on the configured timings and action types.
    /// </summary>
    /// <returns>A list of scheduled NoteActions</returns>
    private List<NoteAction> ScheduleNoteActions() {
        List<NoteAction> ret = new List<NoteAction>();

        List<Pattern4Measures> measureGroups = new List<Pattern4Measures>();
        measureGroups.Add(Pattern.MeasureGroup1);
        measureGroups.Add(Pattern.MeasureGroup2);
        measureGroups.Add(Pattern.MeasureGroup3);
        measureGroups.Add(Pattern.MeasureGroup4);

        List<PatternMeasure> measures = new List<PatternMeasure>();
        foreach (Pattern4Measures measureGroup in measureGroups) {
            measures.Add(measureGroup.Measure1);
            measures.Add(measureGroup.Measure2);
            measures.Add(measureGroup.Measure3);
            measures.Add(measureGroup.Measure4);
        }

        // Keep track of each shot we make
        int shotIndex = -1;
        for (int i = 0; i < measures.Count; i++) {
            for (int j = 0; j < ACTIONS_PER_MEASURE; j++) {                
                string actionString = measures[i].NoteActions[j];
                ConfigurationEvent.Values value = ConfigurationEvent.GetBaseValueForString(actionString);
                if (value == ConfigurationEvent.Values.None) 
                    continue;
                
                int elapsedThirtySecondNotes = i * ACTIONS_PER_MEASURE + j;
                float triggerTime = timingController.GetThirtysecondNoteTime() * elapsedThirtySecondNotes;

                // Type configEventType = measures[i].ConfigEvent.GetType();
                if (measures[i].ConfigEvent is Shot) {
                    if (actionString == Shot.Values.FireShot.ToString()) {
                        shotIndex++;
                        FireShotNoteAction fireShotNote = new FireShotNoteAction(shotIndex, triggerTime, eventInstances,
                            (Shot)measures[i].ConfigEvent, Agent);
                        ret.Add(fireShotNote);
                    } else {
                        // TODO: Currently, we just update the shot most recently timed to fire before this update. It would be nice to be able to update specific shots.
                        Assert.IsTrue(shotIndex > -1, "Trying to update a shot before we have shot any shots, silly!");
                        UpdateShotNoteAction updateShotNote =
                            new UpdateShotNoteAction(shotIndex, triggerTime, eventInstances, actionString);
                        ret.Add(updateShotNote);
                    }
                }

                // check if it's an animation action
                // TODO: Just find the PianoAnimationEvent in the scene for now, but ideally we'd spawn it in
                else if (measures[i].ConfigEvent is AnimationEvent) {
                    PianoAnimationEvent pianoAnimation = FindObjectOfType<PianoAnimationEvent>();
                    // TODO: We aren't using the shotIndex value here, and it is also probably wrong.
                    UpdateAnimationNoteAction updateAnimationNote = new UpdateAnimationNoteAction(shotIndex, triggerTime, pianoAnimation, actionString);
                    ret.Add(updateAnimationNote);
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// Perform any queued NoteActions that have triggered
    /// </summary>
    private void Update() {
        if (GameController.Instance.IsResetting())
            return;
        
        timeElapsed += Time.deltaTime;

        int actionsCompleted = 0;
        for (int i = 0; i < queuedActions.Count; i++) {
            NoteAction noteAction = queuedActions[i];
            if (noteAction.TriggerTime < timeElapsed) {
                noteAction.PerformAction();
                actionsCompleted++;
            } else {
                // Since we assume that the queuedShots list is ordered by FireTime, we know that none of the remaining shots should be fired yet
                break;
            }
        }

        queuedActions.RemoveRange(0, actionsCompleted);
    }
}
