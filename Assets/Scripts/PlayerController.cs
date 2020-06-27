using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public Player Player;

    public Vector3 GetPlayerPosition() {
        return Player.transform.position;
    }
}
