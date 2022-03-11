using UnityEngine;

/// <summary>
/// Git yer info about the player here
/// </summary>
public class PlayerController : MonoBehaviour {
    public Player Player;

    public static PlayerController Instance;

    public static PlayerController GetPlayerController() {
        return Instance;
    }
    void Start() {
        Instance = this;
    }
    
    public Vector3 GetPlayerPosition() {
        return Player.transform.position;
    }

    public Quaternion QuaternionToAimAtPlayer(Vector3 origin) {
        return CalculateRotation(origin, GetPlayerPosition());
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
