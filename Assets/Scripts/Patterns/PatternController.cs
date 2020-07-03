using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class PatternController : MonoBehaviour {
    public Transform Spawner;
    public PatternConfiguration Pattern;

    private GameController gameController;
    private float timeElapsed;

    // The original list of configured actions configured at start
    private List<BaseAction> configuredActions;
    // An updated list originally copied from configuredShots
    private List<BaseAction> queuedActions;

    private List<Shot> shotInstances = new List<Shot>();

    // TODO: We are deciding for now to queue all of the shots when this pattern is instantiated rather than more dynamically determinig when to shoot in the update method. 
    // This is probably fine. 
    private void Start() {
        gameController = FindObjectOfType<GameController>();

        configuredActions = DetermineShotTimings();

        queuedActions = new List<BaseAction>(configuredActions);
    }

    private List<BaseAction> DetermineShotTimings() {
        List<BaseAction> ret = new List<BaseAction>();

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
        List<bool> thirtysecondNotes = new List<bool>();
        for (int i = 0; i < measures.Count; i++) {
            for (int j = 0; j < 32; j++) {  // TODO: Magic number
                int action = measures[i].Notes[j];
                // 0 means "no action"
                if (action > 0) {
                    int elapsedThirtySecondNotes = i * 32 + j;
                    float triggerTime = gameController.GetThirtysecondNoteTime() * elapsedThirtySecondNotes;

                    // 1 means "spawn a bullet"
                    if (action == 1) {
                        shotIndex++;
                        FireShotAction fireShot = new FireShotAction(shotIndex, triggerTime, measures[i].Shot, shotInstances, Spawner);
                        ret.Add(fireShot);
                    } else {
                        // > 1 means "update a shot". Currently, we just update the shot most recently timed to fire before this update
                        Assert.IsTrue(shotIndex > -1, "Trying to update a shot before we have shot any shots, silly!");
                        UpdateShotAction updateShot = new UpdateShotAction(shotIndex, triggerTime, measures[i].Shot, shotInstances, action);
                        ret.Add(updateShot);
                    }                    
                }
            }
        }

        // TODO: Do we need to make sure the return list is ordered by FireTime?
        return ret;
    }

    private void Update() {
        timeElapsed += Time.deltaTime;

        int actionsCompleted = 0;
        for (int i = 0; i < queuedActions.Count; i++) {
            BaseAction action = queuedActions[i];
            if (action.TriggerTime < timeElapsed) {
                action.PerformAction();
                actionsCompleted++;
            } else {
                // Since we assume that the queuedShots list is ordered by FireTime, we know that none of the remaining shots should be fired yet
                break;
            }
        }

        queuedActions.RemoveRange(0, actionsCompleted);
    }
}
