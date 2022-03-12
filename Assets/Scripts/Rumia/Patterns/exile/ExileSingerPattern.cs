using Rumia;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExileSingerPattern : Pattern {
    [StepwiseEmissionRumiaAction]
    public Emitter Melody1;
    [StepwiseEmissionRumiaAction]
    public Emitter Melody2;
    public Emitter Melody3;
    [EmissionRumiaAction]
    public Emitter Melody4;
    private Vector2 currentMelody3Target = new Vector2(0, 0);
    private bool melody3TargetIsCurrentlySet = false;
    public Transform Melody4EmitterAimer;
    

    private void ReAimAndMoveEmittedBullet(Emitter emitter, int index) {
        emitter.TriggerSpeedChange(index);
        emitter.TriggerAimChange(index);
    }
    
    #region RumiaActions

    private void ChangePhaseForMelody(Emitter emitter, int phaseIndex) {
        ReAimAndMoveEmittedBullet(emitter, phaseIndex);
        emitter.UnsubscribeAllSpeedAndAimChangeObjects();
    }

    [RumiaAction]
    [Button]
    public void Melody1ChangePhase(int phaseIndex) {
        ChangePhaseForMelody(Melody1, phaseIndex);
    }

    [RumiaAction]
    [Button]
    public void Melody3EmissionStepwise(int index) {
        if (!melody3TargetIsCurrentlySet) {
            float nextAngleOffset = PlayerController.Instance.QuaternionToAimAtPlayer(transform.position).eulerAngles.z;
            // currentMelody3Target = new Vector2(transform.position.x + distanceAwayToTarget * Mathf.Cos(nextAngleOffset), 
                                            // transform.position.y + distanceAwayToTarget * Mathf.Sin(nextAngleOffset));
            melody3TargetIsCurrentlySet = true;
            Melody4EmitterAimer.Rotate(0, 0, nextAngleOffset);
        }
        Melody3.EmitDetailed(Melody4EmitterAimer.eulerAngles.z, index);
    }
 
    [RumiaAction]
    [Button]
    public void ResetMelody3Target() {
        // currentMelody3Target = Vector2.zero;
        Melody4EmitterAimer.rotation = Quaternion.identity;
        melody3TargetIsCurrentlySet = false;
    }
    
    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Move() {
        
    }

    #endregion
}
