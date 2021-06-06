using UnityEngine;

public class TrailBulletLogic : BulletLogic {
    private TrailRendererWith2DCollider originalTrail;
    private TrailRendererWith2DCollider trailRenderer;
    
    public TrailBulletLogic(TrailRendererWith2DCollider originalTrail) {
        this.originalTrail = originalTrail;
    }

    public override void OnBulletSpawned(Bullet bullet) {
        Transform bulletTransform = bullet.transform;
        trailRenderer = Object.Instantiate(originalTrail, bulletTransform);
        trailRenderer.Initialize();
        // trailRenderer.transform.SetParent(bulletTransform);
        // trailRenderer.transform.SetParent(GameController.Instance.ShotBucket);
    }

    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        // Nothing to do
    }
    
    public override void OnBulletDestroyed(Bullet bullet) {
        if (trailRenderer != null) {
            Object.Destroy(trailRenderer.GetTrail());
        }
    }
}
