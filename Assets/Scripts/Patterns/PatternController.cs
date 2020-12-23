using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Parses through a PatternConfiguration and generates NoteActions based on the configured timings and action types.
/// NoteActions and instantiated and scheduled upon PatternController initialization. 
/// </summary>
public class PatternController : MonoBehaviour {
    private const int ACTIONS_PER_MEASURE = 32;

    public PatternConfiguration Config;

    private TimingController timingController;
    private float timeElapsed;

    // The original list of configured actions configured at start
    private List<NoteAction> configuredActions;
    // An updated list originally copied from configuredShots
    private List<NoteAction> queuedActions;
    // Shot instances are added here when they spawn. We pass this to UpdateNoteActions so that they can access the Shot instances. 
    private Pattern patternInstance;

    private GameController gameController;
    
    private void Start() {
        gameController = GameController.Instance;
        timingController = FindObjectOfType<TimingController>();
        
        // TODO: Better spawning logic pls. Like, actually have it be configurable. It would be real great to not have to spawn this before scheduling note actions
        Transform spawner = gameController.EnemyManager.transform;
        patternInstance = Instantiate(Config.Pattern, Vector2.zero, Quaternion.identity, spawner);
        patternInstance.gameObject.SetActive(false);
        configuredActions = ScheduleNoteActions();
        queuedActions = new List<NoteAction>(configuredActions);
    }

    /// <summary>
    /// Parses through a PatternConfiguration and generates NoteActions based on the configured timings and action types.
    /// </summary>
    /// <returns>A list of scheduled NoteActions</returns>
    private List<NoteAction> ScheduleNoteActions() {
        List<NoteAction> ret = new List<NoteAction>();
        List<PatternMeasureList> measureLists = Config.MeasuresList;
        
        // Iterate through each PatternMeasureList
        for (int i = 0; i < measureLists.Count; i++) {
            if (measureLists[i] == null)
                continue;
            
            // Iterate through each measure
            List<PatternMeasure> measureList = measureLists[i].Measures;
            for (int j = 0; j < measureList.Count; j++) {
                if (measureLists[i].Measures[j] == null)
                    continue;
                
                // Iterate through each instant (32nd note) in the measure
                for (int k = 0; k < ACTIONS_PER_MEASURE; k++) {
                    PatternActionList[] patternActionLists = measureList[j].PatternActionLists;
                    ChoiceParameterList[] choiceParameterLists = measureList[j].ChoiceParameterLists;
                    if (patternActionLists[k] == null)
                        continue;
                    
                    // Iterate through each PatternAction assigned to this instant
                    List<PatternAction> patternActions = patternActionLists[k].PatternActions;
                    List<string> choiceParameters = choiceParameterLists[k].ChoiceParameters;
                    for (int l = 0; l < patternActions.Count; l++) {
                        PatternAction patternAction = patternActions[l];
                        // Get the serialized parameter to pass into the PatternAction when it comes time to invoke it
                        string parameter = choiceParameters[l];

                        // If the action is "None", ignore it
                        if (patternAction.ActionName.Equals(PatternAction.NoneString))
                            continue;

                        // Factor in the start measure, which measure we're currently on, and which part of the measure we're currently on
                        int elapsedThirtySecondNotes =
                            Config.StartMeasure * ACTIONS_PER_MEASURE + i * ACTIONS_PER_MEASURE + k;
                        float triggerTime = timingController.GetThirtysecondNoteTime() * elapsedThirtySecondNotes
                                            + timingController.GetStartDelay();

                        NoteAction noteAction =
                            new NoteAction(triggerTime, patternAction, GetPatternInstance, parameter);
                        ret.Add(noteAction);
                    }
                }
            }
        }

        return ret;
    }

    /// <summary>
    /// Guaranteed to be non-null (and by guaranteed, I mean that it will throw an exception otherwise)
    /// </summary>
    /// <returns></returns>
    private Pattern GetPatternInstance() {
        Assert.IsNotNull(patternInstance);
        return patternInstance;
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
