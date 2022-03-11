using System.Collections;
using System.Collections.Generic;
using Rumia;
using Sirenix.OdinInspector;
using UnityEngine;

public class ExileBiverPattern : Pattern {
    [EmissionRumiaAction]
    public Emitter Melody1;
    public SpeedSubscriptionObject Melody1SpeedSubscription;
    public AimSubscriptionObject Melody1AimSubscription;
    public float Melody1Phase2StartSpeed;
    public AnimationCurve Melody1Phase2SpeedCurve;
    
    // Start is called before the first frame update
    void Start() {
        Melody1SpeedSubscription = new SpeedSubscriptionObject(Melody1Phase2StartSpeed, Melody1Phase2SpeedCurve);
        Melody1.AssignSpeedSubscriptionObject(Melody1SpeedSubscription);
        Melody1AimSubscription = new AimSubscriptionObject();
        Melody1.AssignAimSubscriptionObject(Melody1AimSubscription);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ReAimAndMoveBullet(SpeedSubscriptionObject speed, AimSubscriptionObject aim) {
        speed.TriggerSpeedCurve();
        aim.TriggerRotationTowardsPlayer();
        
    }
    
    #region RumiaActions

    [RumiaAction]
    [Button]
    public void Melody1Phase2() {
        ReAimAndMoveBullet(Melody1SpeedSubscription, Melody1AimSubscription);
    }
    
    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Dummy() {
        Debug.Log("Dummy pattern:                                Biver");
    }

    // ReSharper disable once UnusedMember.Global
    [RumiaAction]
    public void Move() {
        
    }

    #endregion
}
