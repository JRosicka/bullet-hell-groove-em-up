using UnityEngine;

public class TrailBulletLogic : BulletLogic {
    private TrailRendererWith2DCollider originalTrail;
    private TrailRendererWith2DCollider trail;
    
    public TrailBulletLogic(TrailRendererWith2DCollider originalTrail) {
        this.originalTrail = originalTrail;
    }

    public override void OnBulletSpawned(Bullet bullet) {
        Transform bulletTransform = bullet.transform;
        trail = Object.Instantiate(originalTrail, bulletTransform);
        trail.Initialize();
        // trail.transform.SetParent(bulletTransform);
        // trail.transform.SetParent(GameController.Instance.ShotBucket);
    }

    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        // Nothing to do
    }
    
    public override void OnBulletDestroyed(Bullet bullet) {
        if (trail != null) {
            Object.Destroy(trail.GetTrail());
        }
    }
}
