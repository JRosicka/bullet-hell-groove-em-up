using System.Collections;
using System.Collections.Generic;
using Rumia;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExileBiverPattern : Pattern {
    [EmissionRumiaAction]
    public Emitter Melody1;
    
    void Start() {
        
    }

    void Update()
    {
        
    }

    private void ReAimAndMoveEmittedBullet(Emitter emitter, int index) {
        emitter.TriggerSpeedChange(index);
        emitter.TriggerAimChange(index);
    }
    
    #region RumiaActions

    [RumiaAction]
    [Button]
    public void Melody1ChangePhase(int phaseIndex) {
        ReAimAndMoveEmittedBullet(Melody1, phaseIndex);
        Melody1.UnsubscribeAllSpeedAndAimChangeObjects();
    }
    
    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Dummy() {
        Debug.Log("Dummy pattern:                                Biver");
    }

    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Move() {
        
    }

    #endregion
}
