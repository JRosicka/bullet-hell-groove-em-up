using UnityEngine;

/// <summary>
/// Defines behavior for a <see cref="Bullet"/>, passed to the Bullet by the <see cref="Emitter"/> that spawns it.
///
/// This can be subclassed to define all sorts of behavior, like movement bezier curves, animations, timed logic, etc.
/// </summary>
public abstract class BulletLogic {
    public abstract void OnBulletSpawned(Bullet bullet);
    public abstract void BulletLogicUpdate(float deltaTime);
}
