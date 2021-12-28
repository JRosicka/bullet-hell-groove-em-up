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

}
