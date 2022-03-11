using UnityEngine;

/// <summary>
/// TODO - just copied from <see cref="SubscribeToSpeedControllerBulletLogic"/>
/// </summary>
public class SubscribeToAimControllerBulletLogic : BulletLogic {
    private AimSubscriptionObject subscriptionObject;
    private Bullet bulletInstance;
    
    private bool forcedRotationChange;
    
    public SubscribeToAimControllerBulletLogic(AimSubscriptionObject subscriptionObject) {
        this.subscriptionObject = subscriptionObject;
    }
    
    public override void OnBulletSpawned(Bullet bullet) {
        bulletInstance = bullet;
        subscriptionObject.SubscribeBullet(this);
    }

    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        // Nothing to do here
    }

    public override void OnBulletDestroyed(Bullet bullet) {
        subscriptionObject.UnsubscribeBullet(this);
    }

    // TODO: Gross make this degrees around the z axis rather than a quaternion. Will need to modify PlayerController.QuaternionToAimAtPlayer to do the same. 
    public void TriggerRotation(Quaternion rotation) {
        forcedRotationChange = true;
        bulletInstance.transform.rotation = rotation;
        bulletInstance.IsRotationInterrupted = true;
    }

    public void TriggerRotationTowardsPlayer() {
        Quaternion targetRotation = PlayerController
            .GetPlayerController()
            .QuaternionToAimAtPlayer(bulletInstance.transform.position);
        TriggerRotation(targetRotation);
    }
}
