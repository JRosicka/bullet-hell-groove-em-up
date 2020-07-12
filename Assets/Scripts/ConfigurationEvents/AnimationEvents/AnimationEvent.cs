using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationEvent : ConfigurationEvent {
    public new enum Values {
        Unset = ConfigurationEvent.Values.Unset,
        None = ConfigurationEvent.Values.None,
    }
}
