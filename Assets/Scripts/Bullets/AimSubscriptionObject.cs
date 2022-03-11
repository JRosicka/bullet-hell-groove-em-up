using System.Collections.Generic;
using UnityEngine;

public class AimSubscriptionObject {
    private List<SubscribeToAimControllerBulletLogic> subscribers = new List<SubscribeToAimControllerBulletLogic>();
    
    public void SubscribeBullet(SubscribeToAimControllerBulletLogic bullet) {
        subscribers.Add(bullet);
    }

    public void UnsubscribeBullet(SubscribeToAimControllerBulletLogic bullet) {
        subscribers.Remove(bullet);
    }

    public void TriggerRotation(Quaternion newRotation) {
        foreach (SubscribeToAimControllerBulletLogic logic in subscribers) {
            logic.TriggerRotation(newRotation);
        }
    }

    public void TriggerRotationTowardsPlayer() {
        foreach (SubscribeToAimControllerBulletLogic logic in subscribers) {
            logic.TriggerRotationTowardsPlayer();
        }
    }
}
