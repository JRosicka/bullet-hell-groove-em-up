using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour {
    private const string SLOW_NAME = "ctr_slow";
    private const string QUIT_NAME = "ctr_quit";

    private float quitButtonHeldDownLength;
    private const float REQUIRED_TIME_TO_QUIT = 1;

    void Update() {
        Rewired.Player playerControls = ReInput.players.GetPlayer("SYSTEM");

        if (playerControls.controllers.joystickCount == 0) {
            Joystick joystick = ReInput.controllers.GetJoystick(0);
            playerControls.controllers.AddController(joystick, true);
        }

        if (playerControls.GetButton(SLOW_NAME))
            SceneManager.LoadScene("MainScene");
        
        // Check to see if we should quit
        if (playerControls.GetButton(QUIT_NAME)) {
            quitButtonHeldDownLength += Time.deltaTime;
            if (quitButtonHeldDownLength >= REQUIRED_TIME_TO_QUIT) {
                Debug.Log("Quitting");
                Application.Quit();
            }
        } else {
            quitButtonHeldDownLength = 0;
        }
    }
}
