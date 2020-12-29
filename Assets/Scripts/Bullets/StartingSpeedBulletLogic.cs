public class StartingSpeedBulletLogic : BulletLogic {
    public float StartingSpeed;

    public StartingSpeedBulletLogic(float startingSpeed) {
        StartingSpeed = startingSpeed;
    }
    public override void OnBulletSpawned(Bullet bullet) {
        bullet.speed = StartingSpeed;
    }
}
