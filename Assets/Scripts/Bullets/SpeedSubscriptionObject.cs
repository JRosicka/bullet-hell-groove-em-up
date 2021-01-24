using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedSubscriptionObject {
    private List<SubscribeToSpeedControllerBulletLogic> subscribers = new List<SubscribeToSpeedControllerBulletLogic>();
    private float startingSpeed;
    private AnimationCurve speedCurve;

    public SpeedSubscriptionObject(float startingSpeed, AnimationCurve speedCurve) {
        this.startingSpeed = startingSpeed;
        this.speedCurve = speedCurve;
    }

    public void SubscribeBullet(SubscribeToSpeedControllerBulletLogic bullet) {
        subscribers.Add(bullet);
    }

    public void UnsubscribeBullet(SubscribeToSpeedControllerBulletLogic bullet) {
        subscribers.Remove(bullet);
    }

    public void TriggerSpeedCurve() {
        foreach (SubscribeToSpeedControllerBulletLogic logic in subscribers) {
            logic.TriggerSpeedCurve(startingSpeed, speedCurve);
        }
    }
}
