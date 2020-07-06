using UnityEngine;

/// <summary>
/// Git yer info about the player here
/// </summary>
public class PlayerController : MonoBehaviour {
    public Player Player;

    public Vector3 GetPlayerPosition() {
        return Player.transform.position;
    }
}
