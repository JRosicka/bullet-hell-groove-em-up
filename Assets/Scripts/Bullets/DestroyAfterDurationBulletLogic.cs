using UnityEngine;

public class DestroyAfterDurationBulletLogic : BulletLogic {
    private float destroyTime;
    private float elapsedTime;
    
    public DestroyAfterDurationBulletLogic(float destroyTime) {
        this.destroyTime = destroyTime;
    }

    public override void OnBulletSpawned(Bullet bullet) {
        elapsedTime = 0;
    }

    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        elapsedTime += deltaTime;
        if (elapsedTime > destroyTime) {
            Object.Destroy(bullet.gameObject);
        }
    }
}
