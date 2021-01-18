using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

/// <summary>
/// The shot that goes "buh, beDAAAA"
/// Fires three rounds of projectiles that each aim at the player
/// </summary>
public class PianoPattern : Pattern {
    public Animator OuterSegments;
    public Animator InnerSegments;
    
    private GameController gameController;
    private PlayerController playerController;
    private WaypointManager waypointManager;

    private enum MoveType {
        NormalMove,
        MoveAway,
        MoveTowards
    }

    private List<Transform> waypoints = new List<Transform>();
    private List<Transform> nonComboWaypoints = new List<Transform>();
    private List<Transform> middleWaypoints = new List<Transform>();
    
    private bool IsOnRightSide => transform.position.x > 0;
    private bool IsPlayerOnRightSide => playerController.GetPlayerPosition().x > 0;

    [Header("Movement")] 
    public AnimationCurve MovementCurveRegular;
    public AnimationCurve MovementCurveFudge;
    // How long it takes to move from one waypoint to another
    public float TravelTime = 1f;

    public PianoShot ShotPrefab;
    private PianoShot lastShotInstance;

    void Start() {
        gameController = GameController.Instance;
        waypointManager = gameController.WaypointManager;
        playerController = FindObjectOfType<PlayerController>();
        
        waypoints.Add(waypointManager.BottomLeft);
        waypoints.Add(waypointManager.BottomRight);
        waypoints.Add(waypointManager.MiddleLeft);
        waypoints.Add(waypointManager.MiddleRight);
        waypoints.Add(waypointManager.TopLeft);
        waypoints.Add(waypointManager.TopRight);
        waypoints.Add(waypointManager.LeftCombo1);
        waypoints.Add(waypointManager.LeftCombo2);
        waypoints.Add(waypointManager.LeftCombo3);
        waypoints.Add(waypointManager.RightCombo1);
        waypoints.Add(waypointManager.RightCombo2);
        waypoints.Add(waypointManager.RightCombo3);
        
        nonComboWaypoints.Add(waypointManager.BottomLeft);
        nonComboWaypoints.Add(waypointManager.BottomRight);
        nonComboWaypoints.Add(waypointManager.MiddleLeft);
        nonComboWaypoints.Add(waypointManager.MiddleRight);
        nonComboWaypoints.Add(waypointManager.TopLeft);
        nonComboWaypoints.Add(waypointManager.TopRight);
    }
    

    private float currentTravelTime = -1;
    private Vector3 originWaypoint;
    private Vector3 targetWaypoint;
    private Vector3 fudgeTargetWaypoint;
    private bool movedBefore;
    private static readonly int MoveOut = Animator.StringToHash("MoveOut");
    private static readonly int Spin = Animator.StringToHash("Spin");

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
    public void FirstShot() {
        Transform t = transform;
        lastShotInstance = Instantiate(ShotPrefab, t.position, t.rotation,
            GameController.Instance.ShotBucket);
        lastShotInstance.FirstShot();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void SecondShot() {
        lastShotInstance.SecondShot();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void ThirdShot() {
        lastShotInstance.ThirdShot();
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void FireAnimationFlare() {
        OuterSegments.SetTrigger(MoveOut);
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void FireSecondaryAnimationFlare() {
        OuterSegments.SetTrigger(MoveOut);
        InnerSegments.SetTrigger(MoveOut);
    }
    
    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void SpinAnimation() {
        OuterSegments.SetTrigger(Spin);
        InnerSegments.SetTrigger(Spin);
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void NormalMove() {
        MoveToWaypoint(DetermineTargetTransform(MoveType.NormalMove));
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void MoveAway() {
        MoveToWaypoint(DetermineTargetTransform(MoveType.MoveAway));
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void MoveTowards() {
        MoveToWaypoint(DetermineTargetTransform(MoveType.MoveTowards));
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void MoveCombo1() {
        MoveToWaypoint(IsOnRightSide ? waypointManager.RightCombo1 : waypointManager.LeftCombo1);
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void MoveCombo2() {
        MoveToWaypoint(IsOnRightSide ? waypointManager.LeftCombo2 : waypointManager.RightCombo2);
    }

    // ReSharper disable once UnusedMember.Global
    [PatternActionAttribute]
    public void MoveCombo3() {
        MoveToWaypoint(IsOnRightSide ? waypointManager.LeftCombo3 : waypointManager.RightCombo3);
    }
    
    #endregion
    
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

    private Transform DetermineTargetTransform(MoveType moveEvent) {
        Transform ret = null;
        float enemyToPlayer = (transform.position - playerController.GetPlayerPosition()).magnitude;
        List<Transform> sortedWaypoints = GetSortedWaypoints();
        switch (moveEvent) {
            case MoveType.NormalMove:
                ret = sortedWaypoints[Random.Range(0, 2)];
                break;
            case MoveType.MoveTowards:
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
            case MoveType.MoveAway:
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
            // case Values.MoveToMiddle:
            //     // TODO: We never put anything in here lol ;)
            //     ret = middleWaypoints[Random.Range(0, middleWaypoints.Count)];
            //     break;
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
}
