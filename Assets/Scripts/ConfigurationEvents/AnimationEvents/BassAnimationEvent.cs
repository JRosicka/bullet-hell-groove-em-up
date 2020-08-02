using System;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class BassAnimationEvent : AnimationEvent {
    public new enum Values {
        Unset = ConfigurationEvent.Values.Unset,
        None = ConfigurationEvent.Values.None,
        FireAnimationFlare = 200,
        MoveLeft = 201,
        MoveRight = 202,
    }
    
    [Header("Movement")] 
    public AnimationCurve MovementCurveRegular;
    public AnimationCurve MovementCurveFudge;
    // How long it takes to move from one waypoint to another
    public float TravelTime = 1f;

    private float currentTravelTime = -1;
    private Vector3 originWaypoint;
    private Vector3 targetWaypoint;
    private Vector3 fudgeTargetWaypoint;
    private bool movedBefore;
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

    public override void UpdateEvent(string step) {
        Values action = GetValueForString(step);
        switch (action) {
            case Values.FireAnimationFlare:
                FireAnimationFlare();
                break;
            case Values.MoveLeft:
                MoveToWaypoint(DetermineTargetTransform(Values.MoveLeft));
                break;
            case Values.MoveRight:
                MoveToWaypoint(DetermineTargetTransform(Values.MoveRight));
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    private void FireAnimationFlare() {
        // TODO: Animate
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

    private Vector3 DetermineTargetTransform(Values moveEvent) {
        Vector3 ret = Vector3.zero;
        Vector3 currentPosition = transform.position;
        switch (moveEvent) {
            case Values.MoveLeft:
                ret = new Vector3(currentPosition.x - .5f, currentPosition.y);
                break;
            case Values.MoveRight:
                ret = new Vector3(currentPosition.x + .5f, currentPosition.y);
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
        return ret;
    }
    
    public override string[] GetValues() {
        return Enum.GetNames(typeof(Values));
    }

    private Values GetValueForString(string stringName) {
        return (Values)Enum.Parse(typeof(Values), stringName);
    }
}
