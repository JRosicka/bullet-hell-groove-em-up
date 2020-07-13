using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public class PianoAnimationEvent : AnimationEvent {
    public new enum Values {
        Unset = ConfigurationEvent.Values.Unset,
        None = ConfigurationEvent.Values.None,
        FireAnimationFlare = 200,
        NormalMove = 201,
        MoveAway = 202,
        MoveTowards = 203,
        MoveToMiddle = 204,
        MoveCombo1 = 205,
        MoveCombo2 = 206,
        MoveCombo3 = 207
    }
    
    private PlayerController playerController;

    private List<Transform> waypoints = new List<Transform>();
    private List<Transform> nonComboWaypoints = new List<Transform>();
    private List<Transform> middleWaypoints = new List<Transform>();
    
    private bool IsOnRightSide => transform.position.x > 0;
    private bool IsPlayerOnRightSide => playerController.GetPlayerPosition().x > 0;

    [Header("Spawns")]
    public Transform WaypointLeftCombo1;
    public Transform WaypointLeftCombo2;
    public Transform WaypointLeftCombo3;
    public Transform WaypointRightCombo1;
    public Transform WaypointRightCombo2;
    public Transform WaypointRightCombo3;
    public Transform WaypointTopLeft;
    public Transform WaypointTopRight;
    public Transform WaypointBottomLeft;
    public Transform WaypointBottomRight;
    public Transform WaypointMiddleLeft;
    public Transform WaypointMiddleRight;

    [Header("Movement")] 
    public AnimationCurve MovementCurveRegular;
    public AnimationCurve MovementCurveFudge;
    // How long it takes to move from one waypoint to another
    public float TravelTime = 1f;

    void Start() {
        playerController = FindObjectOfType<PlayerController>();
        
        waypoints.Add(WaypointBottomLeft);
        waypoints.Add(WaypointBottomRight);
        waypoints.Add(WaypointLeftCombo1);
        waypoints.Add(WaypointLeftCombo2);
        waypoints.Add(WaypointLeftCombo3);
        waypoints.Add(WaypointMiddleLeft);
        waypoints.Add(WaypointMiddleRight);
        waypoints.Add(WaypointRightCombo1);
        waypoints.Add(WaypointRightCombo2);
        waypoints.Add(WaypointRightCombo3);
        waypoints.Add(WaypointTopLeft);
        waypoints.Add(WaypointTopRight);
        
        nonComboWaypoints.Add(WaypointBottomLeft);
        nonComboWaypoints.Add(WaypointBottomRight);
        nonComboWaypoints.Add(WaypointMiddleLeft);
        nonComboWaypoints.Add(WaypointMiddleRight);
        nonComboWaypoints.Add(WaypointTopLeft);
        nonComboWaypoints.Add(WaypointTopRight);
    }

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
            case Values.MoveCombo1:
                MoveToWaypoint(IsOnRightSide ? WaypointLeftCombo1 : WaypointRightCombo1);
                break;
            case Values.MoveCombo2:
                MoveToWaypoint(IsOnRightSide ? WaypointRightCombo2 : WaypointLeftCombo2);
                break;
            case Values.MoveCombo3:
                MoveToWaypoint(IsOnRightSide ? WaypointRightCombo3 : WaypointLeftCombo3);
                break;
            case Values.NormalMove:
                MoveToWaypoint(DetermineTransform(Values.NormalMove));
                break;
            case Values.MoveAway:
                MoveToWaypoint(DetermineTransform(Values.MoveAway));
                break;
            case Values.MoveTowards:
                MoveToWaypoint(DetermineTransform(Values.MoveTowards));
                break;
            case Values.MoveToMiddle:
                MoveToWaypoint(DetermineTransform(Values.MoveToMiddle));
                break;
            default:
                Assert.IsTrue(false);
                break;
        }
    }

    private void FireAnimationFlare() {
        // TODO: Animate
    }

    private void MoveToWaypoint(Transform waypoint) {
        Vector3 oldTarget = transform.position;
        if (movedBefore)
            oldTarget = targetWaypoint;
        originWaypoint = oldTarget;
        targetWaypoint = waypoint.position;
        Vector3 originalVector = targetWaypoint - originWaypoint;
        Vector3 rotatedVector = new Vector3(originalVector.y, -1 * (originalVector.x), 0);
        int direction = Random.Range(0, 2) == 0 ? 1 : -1;
        fudgeTargetWaypoint = rotatedVector * direction + originWaypoint;
        currentTravelTime = 0;

        movedBefore = true;
    }

    private Transform DetermineTransform(Values moveEvent) {
        Transform ret = null;
        float enemyToPlayer = (transform.position - playerController.GetPlayerPosition()).magnitude;
        List<Transform> sortedWaypoints = GetSortedWaypoints();
        switch (moveEvent) {
            case Values.NormalMove:
                ret = sortedWaypoints[Random.Range(0, 2)];
                break;
            case Values.MoveTowards:
                List<Transform> sortedWayPointsCloserToPlayer = sortedWaypoints.Where(e =>
                    (e.position - playerController.GetPlayerPosition()).magnitude <= enemyToPlayer).ToList();
                if (sortedWayPointsCloserToPlayer.Count <= 0) {
                    // If there are no waypoints left that are closer to the player than we currently are, then just go 
                    // to the closest waypoint
                    ret = sortedWaypoints[0];
                } else {
                    // Pick from one of the three closest waypoints that are closer to the player
                    ret = sortedWayPointsCloserToPlayer[
                        Random.Range(0, Math.Min(2, sortedWayPointsCloserToPlayer.Count))];
                }
                break;
            case Values.MoveAway:
                List<Transform> sortedWayPointsFartherToPlayer = sortedWaypoints.Where(e =>
                    (e.position - playerController.GetPlayerPosition()).magnitude >= enemyToPlayer).ToList();
                if (sortedWayPointsFartherToPlayer.Count <= 0) {
                    // If there are no waypoints left that are farther to the player than we currently are, then just go 
                    // to the closest waypoint
                    ret = sortedWaypoints[0];
                } else {
                    // Pick from one of the three closest waypoints that are farther to the player
                    ret = sortedWayPointsFartherToPlayer[
                        Random.Range(0, Math.Min(2, sortedWayPointsFartherToPlayer.Count))];

                }
                break;
            case Values.MoveToMiddle:
                ret = middleWaypoints[Random.Range(0, middleWaypoints.Count)];
                break;
            default:
                Assert.IsTrue(false);
                break;

        }

        return ret;
    }

    /// <summary>
    /// Makes a waypoints list rearranged in order of shortest distance to the current piano enemy location,
    /// not including the waypoint where we are currently at. Only get non-combo waypoints
    /// </summary>
    private List<Transform> GetSortedWaypoints() {
        return nonComboWaypoints.Where(e => e.transform.position != transform.position)
            .OrderBy(e => (e.position - transform.position).magnitude).ToList();
    }
    
    public override string[] GetValues() {
        return Enum.GetNames(typeof(Values));
    }

    private Values GetValueForString(string stringName) {
        return (Values)Enum.Parse(typeof(Values), stringName);
    }
}
