using System;
using SplineMesh;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Moves a Bullet along a spline over time
/// </summary>
public class MoveAlongSplineBulletLogic : BulletLogic {
    private Spline originalSpline;
    private Spline spline;
    private bool isInPlayMode;
    private bool done;

    public Bullet FollowerPrefab;
    private Bullet follower;
    private bool restartBulletWhenDone;
    private float currentDistanceAlongSpline;
    
    public MoveAlongSplineBulletLogic(Spline originalSpline, bool restartBulletWhenDone) {
        this.originalSpline = originalSpline;
        this.restartBulletWhenDone = restartBulletWhenDone;
    }
    
    public override void OnBulletSpawned(Bullet bullet) {
        follower = bullet;
        if (follower == null)
            throw new Exception("Follow is null, silly. This needs a bullet to move along the spline.");

        follower.IsFollowingSpline = true;
        
        // Make a copy of the original spline and assign it as a child to the bullet GameObject so that its transform is 
        // changed accordingly
        var bulletTransform = bullet.transform;
        spline = Object.Instantiate(originalSpline, bulletTransform.position, bulletTransform.rotation, GameController.Instance.ShotBucket);
        bullet.transform.SetParent(spline.transform);
    }
    
    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        if (done)
            return;
        
        if (currentDistanceAlongSpline < 0)
            return;
        
        currentDistanceAlongSpline += deltaTime * follower.speed;
        
        if (currentDistanceAlongSpline > spline.Length) {
            currentDistanceAlongSpline -= spline.Length;

            if (!restartBulletWhenDone && follower != null) {
                ReleaseBullet();
                return;
            }
        }

        MoveFollower(currentDistanceAlongSpline);
    }
    
    /// <summary>
    /// Detach <see cref="follower"/> from the spline and send it free-floating at its current velocity
    /// </summary>
    private void ReleaseBullet() {
        follower.transform.SetParent(GameController.Instance.ShotBucket, true);
        follower.IsFollowingSpline = false;
        follower = null;
        done = true;
    }

    private void MoveFollower(float newDistanceAlongSpline) {
        if (spline == null || follower == null)
            return;

        CurveSample lengthSample = spline.GetSampleAtDistance(newDistanceAlongSpline);
        // CurveSample lengthSample = spline.GetSample(progress * (spline.nodes.Count - 1));    // For getting the sample weighted by nodes

        var transform = follower.transform;
        Vector3 localPosition = lengthSample.location;
        transform.localPosition = localPosition;
        transform.localRotation = lengthSample.Rotation;
    }
}
