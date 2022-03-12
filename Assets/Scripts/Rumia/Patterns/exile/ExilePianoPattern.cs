using System.Collections;
using System.Collections.Generic;
using Rumia;
using UnityEngine;

public class ExilePianoPattern : Pattern {
    [EmissionRumiaAction]
    public Emitter Rhythm1Emitter1;
    [EmissionRumiaAction]
    public Emitter Rhythm1Emitter2;
    [EmissionRumiaAction]
    public Emitter Rhythm1Emitter3;
    [EmissionRumiaAction]
    public Emitter Rhythm1Emitter4;
    [EmissionRumiaAction]
    public Emitter Rhythm1Emitter5;
    [EmissionRumiaAction]
    public Emitter Rhythm1Emitter6;
    [StepwiseEmissionRumiaAction]
    public Emitter Rhythm2Emitter;
    
    #region RumiaActions
    
    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Dummy() {
        Debug.Log("Dummy pattern: Piano");
    }

    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Move() {
        
    }

    #endregion
}
