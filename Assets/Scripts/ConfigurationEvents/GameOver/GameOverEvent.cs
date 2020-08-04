using System;
using UnityEngine.Assertions;

public class GameOverEvent : ConfigurationEvent {
    public new enum Values {
        Unset = ConfigurationEvent.Values.Unset,
        None = ConfigurationEvent.Values.None,
        GameOver = 300,
    }

    public override void UpdateEvent(string step) {
        Values action = GetValueForString(step);
        switch (action) {
            default:
                Assert.IsTrue(false);
                break;
        }
    }
    
    public override string[] GetValues() {
        return Enum.GetNames(typeof(Values));
    }

    private Values GetValueForString(string stringName) {
        return (Values)Enum.Parse(typeof(Values), stringName);
    }
}
