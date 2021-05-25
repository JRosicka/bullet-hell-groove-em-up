using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rumia {
    /// <summary>
    /// Parses through a <see cref="RumiaConfiguration"/> and generates <see cref="ScheduleNoteActions"/> based on the configured timings and action types.
    /// <see cref="ScheduleNoteActions"/> are instantiated and scheduled upon this object's initialization. 
    /// </summary>
    public class RumiaController : MonoBehaviour {
        private const int ACTIONS_PER_MEASURE = 32;    // 32nd-notes

        public RumiaConfiguration Config;

        private TimingController timingController;
        private float timeElapsed;

        /// <summary>
        /// The original list of configured actions
        /// </summary>
        private List<SchedulableAction> configuredActions;
        /// <summary>
        /// An updated list originally copied from <see cref="configuredActions"/>
        /// </summary>
        private List<SchedulableAction> queuedActions;
        /// <summary>
        /// Instances are added here when they spawn. We pass this to <see cref="ScheduleNoteActions"/> so that they can access the Shot instances. 
        /// </summary>
        private Pattern patternInstance;

        private static GameController gameController => GameController.Instance;
        
        private void Start() {
            timingController = FindObjectOfType<TimingController>();
            
            // We spawn the pattern instance right away but deactivate it. The pattern is responsible for setting it active 
            // again at its scheduled "spawn" time.
            Transform spawner = gameController.EnemyManager.transform;
            patternInstance = Instantiate(Config.Pattern, Vector2.zero, Quaternion.identity, spawner);
            patternInstance.gameObject.SetActive(false);
            configuredActions = ScheduleNoteActions();
            queuedActions = new List<SchedulableAction>(configuredActions);
        }

        /// <summary>
        /// Parses through a <see cref="RumiaConfiguration"/> and generates NoteActions based on the configured timings and action types.
        /// </summary>
        /// <returns>A list of scheduled NoteActions</returns>
        private List<SchedulableAction> ScheduleNoteActions() {
            List<SchedulableAction> ret = new List<SchedulableAction>();
            List<RumiaMeasureList> measureLists = Config.MeasuresList;
            
            // Iterate through each PatternMeasureList
            for (int i = 0; i < measureLists.Count; i++) {
                if (measureLists[i] == null)
                    continue;
                
                // Iterate through each measure
                List<RumiaMeasure> measureList = measureLists[i].Measures;
                for (int j = 0; j < measureList.Count; j++) {
                    if (measureLists[i].Measures[j] == null)
                        continue;
                    
                    // Iterate through each instant (32nd note) in the measure
                    for (int k = 0; k < ACTIONS_PER_MEASURE; k++) {
                        RumiaActionList[] patternActionLists = measureList[j].PatternActionLists;
                        ChoiceParameterList[] choiceParameterLists = measureList[j].ChoiceParameterLists;
                        if (patternActionLists[k] == null)
                            continue;
                        
                        // Iterate through each RumiaAction assigned to this instant
                        List<RumiaAction> patternActions = patternActionLists[k].RumiaActions;
                        List<string> choiceParameters = choiceParameterLists[k].ChoiceParameters;
                        for (int l = 0; l < patternActions.Count; l++) {
                            RumiaAction rumiaAction = patternActions[l];
                            // Get the serialized parameter to pass into the RumiaAction when it comes time to invoke it
                            string parameter = choiceParameters[l];

                            // If the action is "None", ignore it
                            if (rumiaAction.ActionName.Equals(RumiaAction.NoneString))
                                continue;

                            // Factor in the start measure, which measure we're currently on, and which part of the measure we're currently on
                            int elapsedThirtySecondNotes =
                                Config.StartMeasure * ACTIONS_PER_MEASURE + i * ACTIONS_PER_MEASURE + k;
                            float triggerTime = timingController.GetThirtysecondNoteTime() * elapsedThirtySecondNotes;

                            SchedulableAction schedulableAction =
                                new SchedulableAction(triggerTime, rumiaAction, GetPatternInstance, parameter);
                            ret.Add(schedulableAction);
                        }
                    }
                }
            }
            
            // Sort by TriggerTimes
            ret.Sort((act1, act2) => act1.TriggerTime.CompareTo(act2.TriggerTime));

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

            if (GameController.Instance.IsWaitingForStart())
                return;
            
            timeElapsed += Time.deltaTime;

            int actionsCompleted = 0;
            foreach (SchedulableAction schedulableAction in queuedActions) {
                if (schedulableAction.TriggerTime < timeElapsed) {
                    schedulableAction.PerformAction();
                    actionsCompleted++;
                } else {
                    // Since we assume that the queuedActions list is ordered by FireTime, we know that none of the remaining shots should be fired yet
                    break;
                }
            }

            queuedActions.RemoveRange(0, actionsCompleted);
        }
    }
}
