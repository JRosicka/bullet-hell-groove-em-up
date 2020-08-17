using UnityEngine;

public class PianoShot : MonoBehaviour {
    [Header("Spawns")]
    public Transform Spawner1;
    public Transform Spawner2;
    public Transform Spawner3;

    [Header("Particle Systems")]
    public BulletParticleSystem System1;
    public BulletParticleSystem System2;
    public BulletParticleSystem System3;

    private PlayerController playerController;
    
    void Awake() {
        playerController = FindObjectOfType<PlayerController>();
    }
    
    public void FirstShot() {
        Shoot(Spawner1, System1);
    }
    public void SecondShot() {
        Shoot(Spawner2, System2);
    }
    public void ThirdShot() {
        Shoot(Spawner3, System3);
    }
    
    private void Shoot(Transform spawner, BulletParticleSystem system) {
        Vector3 target = playerController.GetPlayerPosition();
        Vector3 origin = spawner.transform.position;

        spawner.rotation = CalculateRotation(origin, target);
        system.Shoot();
    }
    
    /// <summary>
    /// Figure out which way to face in order to aim at the target
    /// </summary>
    /// <param name="origin">Location of the shooter</param>
    /// <param name="target">Location of the target</param>
    private Quaternion CalculateRotation(Vector3 origin, Vector3 target) {
        float angleRad = Mathf.Atan2(target.y - origin.y, target.x - origin.x);
        float angleDeg = (180 / Mathf.PI) * angleRad;
        return Quaternion.Euler(0, 0, angleDeg);
    }

}
