using Rewired;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    private const string RESTART_NAME = "ctr_slow";

    public SongController SongController;
    public EnemyManager EnemyManager;
    public Transform ShotBucket;
    public WaypointManager WaypointManager;
    public TextMesh RestartText;
    public TextMesh SuccessText;
    public float DelayBeforeAllowedToRestart = 2f;
    
    public static GameController Instance;


    public Transform BoundaryRight;
    public Transform BoundaryLeft;
    public Transform BoundaryUp;
    public Transform BoundaryDown;

    private float xMax, xMin, yMax, yMin;

    private bool resettingGame;
    private float elapsedTime;
    private bool won;
    private bool playedVictorySound;
    
    private void Awake() {
        Instance = this;

        xMax = BoundaryRight.position.x;
        xMin = BoundaryLeft.position.x;
        yMax = BoundaryUp.position.y;
        yMin = BoundaryDown.position.y;
    }

    private void Update() {
        if (!resettingGame)
            return;

        // Fade out the music
        SongController.Music.volume -= .001f;
        
        elapsedTime += Time.deltaTime;
        if (!(elapsedTime > DelayBeforeAllowedToRestart)) 
            return;

        if (won) {
            if (SuccessText != null)
                SuccessText.gameObject.SetActive(true);
            if (!playedVictorySound) {
                SongController.PlayVictorySoundEffect();
                playedVictorySound = true;
            }
        } else if (RestartText != null) {
            RestartText.gameObject.SetActive(true);
        }

        Rewired.Player playerControls = ReInput.players.GetPlayer("SYSTEM");
        if (playerControls.GetButton(RESTART_NAME))
            SceneManager.LoadScene("MainScene");
    }

    public void ResetGame(bool won) {
        if (resettingGame)
            return;
        
        resettingGame = true;
        elapsedTime = 0;
        this.won = won;
    }

    public bool IsResetting() {
        return resettingGame;
    }

    public Vector2 EvaluateMove(Vector2 originalMove) {
        Vector2 finalMove = new Vector2();
        finalMove.y = Mathf.Min(Mathf.Max(originalMove.y, yMin), yMax);
        finalMove.x = Mathf.Min(Mathf.Max(originalMove.x, xMin), xMax);
        return finalMove;
    }
}
