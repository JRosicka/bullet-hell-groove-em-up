using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour {
    private const string SLOW_NAME = "ctr_slow";

    void Update() {
        Rewired.Player playerControls = ReInput.players.GetPlayer("SYSTEM");

        if (playerControls.controllers.joystickCount == 0) {
            Joystick joystick = ReInput.controllers.GetJoystick(0);
            playerControls.controllers.AddController(joystick, true);
        }

        if (playerControls.GetButton(SLOW_NAME))
            SceneManager.LoadScene("MainScene");
    }
}
