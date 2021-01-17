using UnityEngine;

public class SpeedOverTimeBulletLogic : BulletLogic {
    public float StartingSpeed;
    private AnimationCurve speedCurve;

    private float elapsedTimeSinceSpawn;

    public SpeedOverTimeBulletLogic(float startingSpeed, AnimationCurve speedCurve) {
        StartingSpeed = startingSpeed;
        this.speedCurve = speedCurve;
    }
    public override void OnBulletSpawned(Bullet bullet) {
        bullet.speed = StartingSpeed;
        elapsedTimeSinceSpawn = 0;
    }

    public override void BulletLogicUpdate(Bullet bullet, float deltaTime) {
        // Update the bullet's current speed
        elapsedTimeSinceSpawn += deltaTime;
        if (speedCurve != null)
            bullet.speed = speedCurve.Evaluate(elapsedTimeSinceSpawn) * StartingSpeed;
        
        // Move the bullet with its updated speed
        bullet.transform.Translate(Vector3.right * (bullet.speed * deltaTime));
    }
}
