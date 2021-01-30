using UnityEngine;

public class PianoShot : MonoBehaviour {
    [Header("Spawns")]
    public Transform Spawner1;
    public Transform Spawner2;
    public Transform Spawner3;
    
    [Header("Emitters")] 
    public Emitter Emitter1;
    public Emitter Emitter2;
    public Emitter Emitter3;
    public Emitter EmitterFirework;
    
    private PlayerController playerController;

    private bool lastShotWasFirework;
    
    void Awake() {
        playerController = FindObjectOfType<PlayerController>();
    }
    
    public void FirstShot() {
        Shoot(Spawner1, Emitter1);
        lastShotWasFirework = false;
    }
    public void SecondShot() {
        Shoot(Spawner2, Emitter2);
        lastShotWasFirework = false;
    }
    public void ThirdShot() {
        if (lastShotWasFirework)
            Spawner3.localPosition = new Vector2(3.1f, 0);
        else {
            Spawner3.localPosition = new Vector2(.75f, 0);
        }
        Shoot(Spawner3, Emitter3);
        lastShotWasFirework = false;
    }

    public void FireworkShot() {
        Shoot(Spawner2, EmitterFirework);
        lastShotWasFirework = true;
    }
    
    private void Shoot(Transform spawner, Emitter emitter) {
        Vector3 target = playerController.GetPlayerPosition();
        Vector3 origin = spawner.transform.position;

        spawner.rotation = CalculateRotation(origin, target);
        emitter.Emit();
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
