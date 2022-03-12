using System;
using Rumia;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExileSingerPattern : Pattern {
    public AnimationCurve SlowMoveCurve;
    public float SlowMoveTimeLength;
    public AnimationCurve FastMoveCurve;
    public float FastMoveTimeLength;
    
    public Emitter Verse1Emitter1;
    public Emitter Verse1Emitter2;
    public Emitter Verse1Emitter3;
    [EmissionRumiaAction]
    public Emitter Verse1Emitter4;

    [StepwiseEmissionRumiaAction]
    public Emitter Melody1;
    [StepwiseEmissionRumiaAction]
    public Emitter Melody2;
    public Emitter Melody3;
    [EmissionRumiaAction]
    public Emitter Melody4;
    protected Vector2 currentMelody3Target = new Vector2(0, 0);
    protected bool melody3TargetIsCurrentlySet = false;
    public Transform Melody4EmitterAimer;
    

    private void ReAimAndMoveEmittedBullet(Emitter emitter, int index) {
        emitter.TriggerSpeedChange(index);
        emitter.TriggerAimChange(index);
    }

    private Vector2 moveOrigin;
    private Vector2 moveDestination;
    private AnimationCurve moveCurve;
    private float moveEndTime;
    private float moveCurrentTimeAmount;
    private void Update() {
        if (Time.time > moveEndTime || moveCurve == null) {
            moveCurve = null;
            return;
        }

        moveCurrentTimeAmount += Time.deltaTime;
        float moveProgress = moveCurve.Evaluate(moveCurrentTimeAmount);
        transform.localPosition = Vector2.Lerp(moveOrigin, moveDestination, moveProgress);
    } 

    #region RumiaActions
    
    [RumiaAction]
    public void Verse1MainStepwiseEmission(int step) {
        Verse1Emitter1.EmitStepwise(step);
        Verse1Emitter2.EmitStepwise(step);
        Verse1Emitter3.EmitStepwise(step);
    }

    protected void ChangePhaseForMelody(Emitter emitter, int phaseIndex) {
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
    [Button]
    public void MoveSlow(Vector2 destination) {
        moveOrigin = transform.localPosition;
        moveDestination = destination;
        moveCurve = SlowMoveCurve;
        moveEndTime = Time.time + SlowMoveTimeLength;
        moveCurrentTimeAmount = 0;
    }
    
    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    [Button]
    public void MoveFast(Vector2 destination) {
        moveOrigin = transform.localPosition;
        moveDestination = destination;
        moveCurve = FastMoveCurve;
        moveEndTime = Time.time + FastMoveTimeLength;
        moveCurrentTimeAmount = 0;
    } 

    #endregion
}
