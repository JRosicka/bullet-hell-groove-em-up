using System;

namespace Rumia {
    /// <summary>
    /// A SchedulableAction is a wrapper around a <see cref="RumiaAction"/> that contains a configurable trigger time and
    /// a means to invoke the configured method on the corresponding <see cref="Pattern"/> instance.
    /// It is created upon initialization of the <see cref="Pattern"/> at runtime.  
    /// </summary>
    public class SchedulableAction {
        public readonly float TriggerTime;
        private RumiaAction rumiaAction;
        private Func<Pattern> patternInstanceGetter;
        private string serializedParameter;

        public SchedulableAction(float triggerTime, RumiaAction rumiaAction, Func<Pattern> patternInstanceGetter,
            string serializedParameter) {
            TriggerTime = triggerTime;
            this.rumiaAction = rumiaAction;
            this.patternInstanceGetter = patternInstanceGetter;
            this.serializedParameter = serializedParameter;
        }

        /// <summary>
        /// Generic "Do the thing" method called when the TriggerTime expires
        /// </summary>
        public void PerformAction() {
            Pattern patternInstance = patternInstanceGetter.Invoke();
            patternInstance.InvokePatternAction(rumiaAction.ID, serializedParameter);
        }
    }
}