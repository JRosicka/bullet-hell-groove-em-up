using System;
using System.Collections;
using System.Collections.Generic;
using SplineMesh;
using UnityEditor;
using UnityEngine;
using UnityEngine.PlayerLoop;

/// <summary>
/// Moves a Bullet along a spline over time
/// </summary>
[ExecuteInEditMode]
[RequireComponent(typeof(Spline))]
public class MoveAlongSplineBulletLogic : MonoBehaviour {
    private Spline spline;
    private float progress = 0;
    private bool isInPlayMode;
    private bool done;
    private float bulletSpeed;

    public Bullet FollowerPrefab;
    private Bullet follower;
    public float DurationInSeconds;
    public bool RespawnBulletWhenDone;

    #if UNITY_EDITOR
    // public bool MoveInEditMode;    // Probably won't work since we need the bullet to move in play mode so that we can access controllers and stuff
    #endif
    
    private void OnEnable() {
        progress = 0;
        spline = GetComponent<Spline>();
        if (follower == null)
            return;
#if UNITY_EDITOR
        EditorApplication.update += EditorUpdate;
#endif
    }

    private void Start() {
        if (Application.IsPlaying(this))
            isInPlayMode = true;

        SpawnBullet();
    }

    private void OnDisable() {
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
#endif
    }

    void EditorUpdate() {
        // if (spline == null || Follower == null)
        //     return;

        // if (!MoveInEditMode)
        return;
            
        DoUpdate();
    }

    void Update() {
        
        // if (spline == null || Follower == null) {
        //     return;
        //     // string excString = spline == null ? "Spline is null!" : "";
        //     // excString += Follower == null ? " Follower is null!" : "";
        //     // throw new Exception(excString);
        // }
        if (isInPlayMode)
            DoUpdate();
    }
    
    private void DoUpdate() {
        if (done)
            return;
        
        if (DurationInSeconds <= 0)
            return;
        
        progress += Time.deltaTime / DurationInSeconds;
        if (progress > 1) {
            progress--;

            if (RespawnBulletWhenDone) {
                if (follower == null)
                    follower = Instantiate(FollowerPrefab, transform);
            } else {
                ReleaseBullet();
            }
        }

        MoveFollower(Time.deltaTime);
    }

    private void SpawnBullet() {
        follower = Instantiate(FollowerPrefab, transform);
        follower.speed = 0;
    }
    
    /// <summary>
    /// Detach <see cref="follower"/> from the spline and send it free-floating at its current velocity
    /// </summary>
    private void ReleaseBullet() {
        follower.transform.SetParent(GameController.Instance.ShotBucket, true);
        follower.speed = bulletSpeed;
        follower = null;
        done = true;
    }

    private void MoveFollower(float deltaTime) {
        if (spline == null || follower == null)
            return;

        // CurveSample lengthSample = spline.GetSampleAtDistance(progress * spline.Length);
        CurveSample lengthSample = spline.GetSample(progress * (spline.nodes.Count - 1));    // For getting the sample weighted by nodes
        var transform1 = follower.transform;
        var localPosition = transform1.localPosition;
        
        Vector3 initialPosition = localPosition;
        localPosition = lengthSample.location;
        transform1.localPosition = localPosition;
        transform1.localRotation = lengthSample.Rotation;

        float deltaPosition = (localPosition - initialPosition).magnitude;
        bulletSpeed = deltaPosition / deltaTime;
    }

    // public override void OnBulletSpawned(Bullet bullet) {
    //     throw new System.NotImplementedException();
    // }
}
