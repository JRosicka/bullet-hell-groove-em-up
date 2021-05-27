using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rumia {
    /// <summary>
    /// Parses through a <see cref="RumiaConfiguration"/> and generates <see cref="ScheduleActions"/> based on the configured timings and action types.
    /// <see cref="ScheduleActions"/> are instantiated and scheduled upon this object's initialization. 
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
        /// Instances are added here when they spawn. We pass this to <see cref="ScheduleActions"/> so that they can access the Shot instances. 
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
            
            SetStartMeasure(gameController.TimingController.NumberOfMeasuresToSkipOnStart);

            configuredActions = ScheduleActions();
            queuedActions = new List<SchedulableAction>(configuredActions);
        }

        /// <summary>
        /// Parses through a <see cref="RumiaConfiguration"/> and generates NoteActions based on the configured timings and action types.
        /// </summary>
        private List<SchedulableAction> ScheduleActions() {
            List<RumiaMeasure> startMeasures = Config.StartMeasures;
            List<SchedulableAction> ret = new List<SchedulableAction>();
            List<RumiaMeasureList> measureLists = Config.MeasuresList;
            
            // Add the start measures
            ret.AddRange(ScheduleRumiaMeasureList(startMeasures, 0, true));

            // Add each PatternMeasureList
            for (int i = 0; i < measureLists.Count; i++) {
                if (measureLists[i] == null)
                    continue;
                
                // Iterate through each measure
                List<RumiaMeasure> measureList = measureLists[i].Measures;
                ret.AddRange(ScheduleRumiaMeasureList(measureList, i, false));
            }
            
            // Sort by TriggerTimes
            ret.Sort((act1, act2) => act1.TriggerTime.CompareTo(act2.TriggerTime));

            return ret;
        }

        /// <summary>
        /// Schedules a collection of <see cref="RumiaMeasure"/>s that each start at the same <see cref="measureNumber"/>
        /// </summary>
        /// <param name="forceOnStartMeasure">If true, then modify the trigger time of each action to be on the first measure
        /// that is played. If false, skip the action if its trigger time is before the start time of the first measure.</param>
        private IEnumerable<SchedulableAction> ScheduleRumiaMeasureList(IEnumerable<RumiaMeasure> measures, int measureNumber, bool forceOnStartMeasure) {
            List<SchedulableAction> ret = new List<SchedulableAction>();
            
            // Iterate through each measure
            foreach (RumiaMeasure measure in measures.Where(measure => measure != null)) {
                // Iterate through each instant (32nd note) in the measure
                for (int k = 0; k < ACTIONS_PER_MEASURE; k++) {
                    RumiaActionList[] patternActionLists = measure.PatternActionLists;
                    ChoiceParameterList[] choiceParameterLists = measure.ChoiceParameterLists;
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
                            Config.StartMeasure * ACTIONS_PER_MEASURE + measureNumber * ACTIONS_PER_MEASURE + k;
                        float triggerTime = timingController.GetThirtysecondNoteTime() * elapsedThirtySecondNotes;

                        // This could happen if we have set a non-zero start time
                        if (triggerTime < timeElapsed) {
                            if (forceOnStartMeasure)
                                triggerTime = timeElapsed;
                            else
                                continue;
                        }
                        
                        SchedulableAction schedulableAction =
                            new SchedulableAction(triggerTime, rumiaAction, GetPatternInstance, parameter);
                        ret.Add(schedulableAction);
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

        private void SetStartMeasure(int startMeasure) {
            timeElapsed = timingController.GetWholeNoteTime() * startMeasure;
            // queuedActions = queuedActions.SkipWhile(e => e.TriggerTime < skipSeconds).ToList();
        }
    }
}
