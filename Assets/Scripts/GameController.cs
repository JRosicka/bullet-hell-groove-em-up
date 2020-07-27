using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    private const string RESTART_NAME = "ctr_slow";

    public SongController SongController;
    public TextMesh RestartText;
    public float DelayBeforeAllowedToRestart = 2f;
    
    public static GameController Instance;

    private bool resettingGame;
    private float elapsedTime;
    
    private void Start() {
        Instance = this;
    }

    private void Update() {
        if (!resettingGame)
            return;

        // Fade out the music
        SongController.Music.volume -= .001f;

        
        elapsedTime += Time.deltaTime;
        if (!(elapsedTime > DelayBeforeAllowedToRestart)) 
            return;
        
        RestartText.gameObject.SetActive(true);

        Rewired.Player playerControls = ReInput.players.GetPlayer("SYSTEM");
        if (playerControls.GetButton(RESTART_NAME))
            SceneManager.LoadScene("MainScene");
    }

    public void ResetGame() {
        resettingGame = true;
        elapsedTime = 0;
    }

    public bool IsResetting() {
        return resettingGame;
    }
}
