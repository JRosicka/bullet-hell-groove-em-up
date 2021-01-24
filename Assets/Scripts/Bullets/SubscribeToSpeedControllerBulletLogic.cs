using UnityEngine;

/// <summary>
/// Subscribes the bullet to a controller for dictating an interruption to the bullet's regular speed over time logic.
/// This is intended as an easy means to pass a temporary speed animation curve to a group of bullets so that their speeds
/// are evaluated along the same curve at the same time.
///
/// Note that the actual bullet movement logic is handled elsewhere (<see cref="SpeedOverTimeBulletLogic"/> and
/// <see cref="MoveAlongSplineBulletLogic"/>.
///
/// Also note that we do not "pause" <see cref="SpeedOverTimeBulletLogic"/> during the run duration of the interrupted speed
/// curve. So, when the interrupted speed curve is finished, the bullet's speed will not necessarily return to the same
/// speed it was at the beginning of the speed interruption (unless those speed happen to match).
/// </summary>
public class SubscribeToSpeedControllerBulletLogic : BulletLogic {
    private SpeedSubscriptionObject subscriptionObject;
    private float startingSpeed;
    private AnimationCurve speedCurve;
    private Bullet bulletInstance;
    
    private bool isFollowingSpeedCurve;
    private float elapsedTimeSinceTrigger;
    
    public SubscribeToSpeedControllerBulletLogic(SpeedSubscriptionObject subscriptionObject) {
        this.subscriptionObject = subscriptionObject;
    }
    
    public override void OnBulletSpawned(Bullet bullet) {
        bulletInstance = bullet;
        subscriptionObject.SubscribeBullet(this);
    }

    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        if (isFollowingSpeedCurve) {
            elapsedTimeSinceTrigger += deltaTime;

            if (elapsedTimeSinceTrigger > speedCurve[speedCurve.length - 1].time) {
                isFollowingSpeedCurve = false;
                elapsedTimeSinceTrigger = 0;
                bullet.IsSpeedLogicInterrupted = false;
            }
            
            if (speedCurve != null)
                bullet.speed = speedCurve.Evaluate(elapsedTimeSinceTrigger) * startingSpeed;

            // Do not move the bullet here - let other BulletLogic handle that
        }
    }

    public override void OnBulletDestroyed(Bullet bullet) {
        subscriptionObject.UnsubscribeBullet(this);
    }

    public void TriggerSpeedCurve(float start, AnimationCurve curve) {
        isFollowingSpeedCurve = true;
        startingSpeed = start;
        speedCurve = curve;
        bulletInstance.IsSpeedLogicInterrupted = true;
    }
}
