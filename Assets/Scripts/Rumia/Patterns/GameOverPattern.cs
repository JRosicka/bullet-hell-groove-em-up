﻿namespace Rumia {
    /// <summary>
    /// Triggers a game-over state when the player wins the game
    /// </summary>
    public class GameOverPattern : Pattern {
        // ReSharper disable once UnusedMember.Global
        [RumiaAction]
        public void GameOver() {
            GameController.Instance.ResetGame(true);
        }
    }
}