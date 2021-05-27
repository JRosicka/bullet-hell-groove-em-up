using System;
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
    public TimingController TimingController;
    
    public static GameController Instance;


    public Transform BoundaryRight;
    public Transform BoundaryLeft;
    public Transform BoundaryUp;
    public Transform BoundaryDown;

    private float xMax, xMin, yMax, yMin;

    private bool resettingGame;
    private float delaySecondsBeforeStart;
    private float elapsedStartDelayTime;
    private float elapsedResetTime;
    private bool won;
    private bool playedVictorySound;
    private bool started;
    
    private void Awake() {
        Instance = this;

        delaySecondsBeforeStart = TimingController.GetStartDelay();

        if (BoundaryRight != null)
            xMax = BoundaryRight.position.x;
        if (BoundaryLeft != null)
            xMin = BoundaryLeft.position.x;
        if (BoundaryUp != null)
            yMax = BoundaryUp.position.y;
        if (BoundaryDown != null)
            yMin = BoundaryDown.position.y;
    }

    private void Update() {
        if (resettingGame)
            HandleReset();
        else if (!started && elapsedStartDelayTime <= delaySecondsBeforeStart)
            elapsedStartDelayTime += Time.deltaTime;
        else if (!started) {
            started = true;
        }
            
    }

    private void HandleReset() {
        elapsedResetTime += Time.deltaTime;

        // Fade out the music
        SongController.Music.volume -= .001f;
        
        if (!(elapsedResetTime > TimingController.DelaySecondsBeforeAllowedToRestart)) 
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
        elapsedResetTime = 0;
        this.won = won;
    }

    public bool IsResetting() {
        return resettingGame;
    }

    public bool IsWaitingForStart() {
        return !started;
    }

    public Vector2 EvaluateMove(Vector2 originalMove, Vector2 currentPosition) {
        Vector2 finalMove = originalMove;
        if (currentPosition.y <= yMin)
            finalMove.y = Math.Max(originalMove.y, 0);
        else if (currentPosition.y >= yMax)
            finalMove.y = Math.Min(originalMove.y, 0);
        if (currentPosition.x <= xMin)
            finalMove.x = Math.Max(originalMove.x, 0);
        else if (currentPosition.x >= xMax)
            finalMove.x = Math.Min(originalMove.x, 0);
            
        return finalMove;
    }

    private const int MAX_SORTING_ORDER = 32767;
    private int lastSortingOrder = 1;
    /// <summary>
    /// Generates and returns a new sorting order. Intended to give a higher value than all previously generated values,
    /// but also loops back to 0 if the max is reached
    /// </summary>
    /// <returns></returns>
    public int GetNewSortingOrder() {
        lastSortingOrder++;
        // Give a cushion of 100 in case we ever need it
        if (lastSortingOrder >= MAX_SORTING_ORDER - 100)
            lastSortingOrder = 1;
        
        return lastSortingOrder;
    }
}
