using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class BassPattern : Pattern {
    private static readonly Vector2 Origin = new Vector2(0, 3);
    private static readonly float MaxDirectionalDistanceFromOrigin = .8f;
    private static readonly float MaxDirectionalMovementPerMove = .7f;
    private static readonly float MinDirectionalMovementPerMove = .2f;
    float xMin => Origin.x - MaxDirectionalDistanceFromOrigin;
    float xMax => Origin.x + MaxDirectionalDistanceFromOrigin;
    float yMin => Origin.y - MaxDirectionalDistanceFromOrigin;
    float yMax => Origin.y + MaxDirectionalDistanceFromOrigin;

    private enum MoveDirections {
        MoveLeft = 201,
        MoveRight = 202,
    }

    public SynthResponseShot SynthShotPrefab;
    
    public Transform Emitters;
    public Animator HorizontalSegments;
    public Animator VerticalSegments;
    
    [Header("Movement")] 
    public AnimationCurve MovementCurveRegular;
    public AnimationCurve MovementCurveFudge;
    // How long it takes to move from one waypoint to another
    public float TravelTime = 1f;
    
    
    public Emitter EmitterM1B1N1;
    public Emitter EmitterM1B1N2;
    public Emitter EmitterM1B1N3;
    public Emitter EmitterM1B3N1;
    public Emitter EmitterM1B3N2;
    public Emitter EmitterM1B3N3;
    public Emitter EmitterM1B3N4;
    public Emitter EmitterM2B1N1;
    public Emitter EmitterM2B1N2;
    public Emitter EmitterM2B1N3;
    public Emitter EmitterM2B3N1;
    public Emitter EmitterM2B3N2;
    public Emitter EmitterM2B3N3;
    public Emitter EmitterM2B3N4;
    public Emitter EmitterM2B3N5;
    public Emitter EmitterM2B3N6;
    public Emitter EmitterM4B3N1;


    public Emitter SynthEmitterClockwise;
    public Emitter SynthEmitterCounterclockwise;

    
    private float currentTravelTime = -1;
    private Vector3 originWaypoint;
    private Vector3 targetWaypoint;
    private Vector3 fudgeTargetWaypoint;
    private bool movedBefore;
    private static readonly int MoveOut = Animator.StringToHash("MoveOut");
    
    private SpeedSubscriptionObject speedSubscriptionObject;
    public float speedInterruptionStartingSpeed;
    public AnimationCurve speedInterruptionCurve;

    private void Awake() {
        if (speedSubscriptionObject == null)
            speedSubscriptionObject = new SpeedSubscriptionObject(speedInterruptionStartingSpeed, speedInterruptionCurve);
    }

    private void Start() {
        speedSubscriptionObject = new SpeedSubscriptionObject(speedInterruptionStartingSpeed, speedInterruptionCurve);
        
        // Leave out EmitterM4B3N1 since that already does a *bweah- BWAH*
        List<Emitter> emittersToSubscribeToSpeedController = new List<Emitter>() {
            EmitterM1B1N1, EmitterM1B1N2, EmitterM1B1N3, EmitterM1B3N1, EmitterM1B3N2, EmitterM1B3N3, 
            EmitterM1B3N4, EmitterM2B1N1, EmitterM2B1N2, EmitterM2B1N3, EmitterM2B3N1, EmitterM2B3N2, 
            EmitterM2B3N3, EmitterM2B3N4, EmitterM2B3N5, EmitterM2B3N6
        };

        foreach (Emitter emitter in emittersToSubscribeToSpeedController) {
            emitter.AssignSpeedSubscriptionObject(speedSubscriptionObject);
        }
    }

    private void Update() {
        if (currentTravelTime < 0)
            return;
        
        currentTravelTime += Time.deltaTime;
        if (currentTravelTime >= TravelTime) {
            transform.position = targetWaypoint;
            currentTravelTime = -1;
            return;
        }

        float normalTravelProgress = MovementCurveRegular.Evaluate(currentTravelTime / TravelTime);
        float fudgeTravelProgress = MovementCurveFudge.Evaluate(currentTravelTime / TravelTime);

        Vector3 normalProgress = Vector3.Lerp(originWaypoint, targetWaypoint, normalTravelProgress) - originWaypoint;
        Vector3 fudgeProgress = Vector3.Lerp(originWaypoint, fudgeTargetWaypoint, fudgeTravelProgress) - originWaypoint;
        Vector3 totalProgress = normalProgress + fudgeProgress;
        transform.position = originWaypoint + totalProgress;
    }
    
    #region PatternActions

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void FireAnimationFlareHorizontal() {
        HorizontalSegments.SetTrigger(MoveOut);
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void FireAnimationFlareVertical() {
        VerticalSegments.SetTrigger(MoveOut);
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void FireSynthShotClockwise() {
        FireSynthShot(true);
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void FireSynthShotCounterClockwise() {
        FireSynthShot(false);
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void MoveLeft() {
        MoveToWaypoint(DetermineTargetTransform(MoveDirections.MoveLeft));
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void MoveRight() {
        MoveToWaypoint(DetermineTargetTransform(MoveDirections.MoveRight));
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM1B1N1() {
        RandomRotate();
        EmitterM1B1N1.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM1B1N2() {
        EmitterM1B1N2.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM1B1N3() {
        EmitterM1B1N3.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM1B3N1() {
        RandomRotate();
        EmitterM1B3N1.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM1B3N2() {
        EmitterM1B3N2.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM1B3N3() {
        EmitterM1B3N3.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM1B3N4() {
        EmitterM1B3N4.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B1N1() {
        RandomRotate();
        EmitterM2B1N1.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B1N2() {
        EmitterM2B1N2.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B1N3() {
        EmitterM2B1N3.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B3N1() {
        RandomRotate();
        EmitterM2B3N1.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B3N2() {
        EmitterM2B3N2.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B3N3() {
        EmitterM2B3N3.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B3N4() {
        EmitterM2B3N4.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B3N5() {
        EmitterM2B3N5.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM2B3N6() {
        EmitterM2B3N6.Emit();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BassShotM4B3N1() {
        RotateToDefault();
        EmitterM4B3N1.Emit();
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void BulletSlowdownEffect() {
        speedSubscriptionObject.TriggerSpeedCurve();
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void SynthShotSmolClockwise() {
        
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void SynthShotIntroClockwise() {
        
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void SynthShotIntroCounterClockwise() {
        
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void HookFlare() {
        
    }

    #endregion
    
    private void FireSynthShot(bool clockwise) {
        if (clockwise)
            SynthEmitterClockwise.Emit();
        else
            SynthEmitterCounterclockwise.Emit();
    }

    private void RandomRotate() {
        Emitters.Rotate(0, 0, Random.Range(0, 360));
    }

    private void RotateToDefault() {
        Emitters.localRotation = Quaternion.identity;
    }
    
    private void MoveToWaypoint(Vector3 waypoint) {
        Vector3 oldTarget = transform.position;
        if (movedBefore)
            oldTarget = targetWaypoint;
        originWaypoint = oldTarget;
        targetWaypoint = waypoint;
        Vector3 originalVector = targetWaypoint - originWaypoint;
        Vector3 rotatedVector = new Vector3(originalVector.y, -1 * (originalVector.x), 0);
        int direction = Random.Range(0, 2) == 0 ? 1 : -1;
        fudgeTargetWaypoint = rotatedVector * direction + originWaypoint;
        currentTravelTime = 0;

        movedBefore = true;
    }

    private Vector3 DetermineTargetTransform(MoveDirections moveEvent) {
        Vector3 ret = Vector3.zero;
        Vector3 currentPosition = transform.position;
        float xTravel;
        float yTravel;
        
        // We want to pick a random travel distance out of the legal range. That way, the agent will tend to pick 
        // movements that go towards the origin
        switch (moveEvent) {
            case MoveDirections.MoveLeft:
                xTravel =
                    Random.Range(Mathf.Max(-MinDirectionalMovementPerMove, xMin - currentPosition.x),
                        Mathf.Min(-MaxDirectionalMovementPerMove, -MinDirectionalMovementPerMove));
                yTravel =
                    Random.Range(Mathf.Max(-MaxDirectionalMovementPerMove, yMin - currentPosition.y),
                        Mathf.Min(MaxDirectionalMovementPerMove, yMax - currentPosition.y));
                ret = new Vector3(currentPosition.x + xTravel, currentPosition.y + yTravel);
                break;
            case MoveDirections.MoveRight:
                xTravel =
                    Random.Range(Mathf.Max(MinDirectionalMovementPerMove, MinDirectionalMovementPerMove), Mathf.Min(MaxDirectionalMovementPerMove, xMax - currentPosition.x));
                yTravel =
                    Random.Range(Mathf.Max(-MaxDirectionalMovementPerMove, yMin - currentPosition.y),
                        Mathf.Min(MaxDirectionalMovementPerMove, yMax - currentPosition.y));
                ret = new Vector3(currentPosition.x + xTravel, currentPosition.y + yTravel);
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
        return ret;
    }
}
