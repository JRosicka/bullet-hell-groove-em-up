using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOverNoteAction : NoteAction {
    private Transform spawner;
    private GameOverEvent gameOverPrefab;

    public GameOverNoteAction(int index, float triggerTime, GameOverEvent gameOverPrefab, Transform spawner) : base(index, triggerTime) {
        this.gameOverPrefab = gameOverPrefab;
        this.spawner = spawner;
    }
    
    public override void PerformAction() {
        GameController.Instance.ResetGame(true);
        // Object.Instantiate(gameOverPrefab, spawner.position, spawner.rotation, PlayerController.Instance.ShotBucket);
    }
}
