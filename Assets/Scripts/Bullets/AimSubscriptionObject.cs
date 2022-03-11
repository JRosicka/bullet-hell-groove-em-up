using System.Collections.Generic;
using UnityEngine;

public class AimSubscriptionObject {
    private List<SubscribeToAimControllerBulletLogic> subscribers = new List<SubscribeToAimControllerBulletLogic>();
    private bool aimAtPlayer;
    private float newRotationDegrees;

    public AimSubscriptionObject(bool aimAtPlayer, float newRotationDegrees) {
        this.aimAtPlayer = aimAtPlayer;
        this.newRotationDegrees = newRotationDegrees;
    }
    
    public void SubscribeBullet(SubscribeToAimControllerBulletLogic bullet) {
        subscribers.Add(bullet);
    }

    public void UnsubscribeBullet(SubscribeToAimControllerBulletLogic bullet) {
        subscribers.Remove(bullet);
    }

    public void TriggerRotation() {
        if (aimAtPlayer) {
            TriggerRotationTowardsPlayer();
        } else {
            TriggerRotation(newRotationDegrees);
        }
    }
    
    private void TriggerRotation(float degrees) {
        foreach (SubscribeToAimControllerBulletLogic logic in subscribers) {
            logic.TriggerRotation(Quaternion.Euler(0, 0, degrees));
        }
    }

    private void TriggerRotationTowardsPlayer() {
        foreach (SubscribeToAimControllerBulletLogic logic in subscribers) {
            logic.TriggerRotationTowardsPlayer();
        }
    }
}
