using System.Collections;
using System.Collections.Generic;
using Rumia;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExileTswiftPattern : ExileSingerPattern {
    [StepwiseEmissionRumiaAction]
    public Emitter SecondMelody1;
    [StepwiseEmissionRumiaAction]
    public Emitter SecondMelody2;
    public Emitter SecondMelody3;
    [EmissionRumiaAction]
    public Emitter SecondMelody4;

    public Emitter Verse2Emitter1;
    public Emitter Verse2Emitter2;
    public Emitter Verse2Emitter3;
    [EmissionRumiaAction]
    public Emitter Verse2Emitter4;

    #region RumiaActions

    [RumiaAction]
    public void Verse2MainStepwiseEmission(int step) {
        Verse2Emitter1.EmitStepwise(step);
        Verse2Emitter2.EmitStepwise(step);
        Verse2Emitter3.EmitStepwise(step);
    }  

    [RumiaAction]
    [Button]
    public void SecondMelody1ChangePhase(int phaseIndex) {
        ChangePhaseForMelody(SecondMelody1, phaseIndex);
    }

    [RumiaAction]
    [Button]
    public void SecondMelody3EmissionStepwise(int index) {
        if (!melody3TargetIsCurrentlySet) {
            float nextAngleOffset = PlayerController.Instance.QuaternionToAimAtPlayer(transform.position).eulerAngles.z;
            // currentMelody3Target = new Vector2(transform.position.x + distanceAwayToTarget * Mathf.Cos(nextAngleOffset), 
            // transform.position.y + distanceAwayToTarget * Mathf.Sin(nextAngleOffset));
            melody3TargetIsCurrentlySet = true;
            Melody4EmitterAimer.Rotate(0, 0, nextAngleOffset);
        }
        SecondMelody3.EmitDetailed(Melody4EmitterAimer.eulerAngles.z, index);
    }
    
    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Dummy() {
        Debug.Log("Dummy pattern:                Tswift");
    }

    #endregion
}