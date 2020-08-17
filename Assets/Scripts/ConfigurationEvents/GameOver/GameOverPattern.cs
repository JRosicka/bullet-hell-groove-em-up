public class GameOverPattern : Pattern {
    // ReSharper disable once UnusedMember.Global
    public void GameOver() {
        GameController.Instance.ResetGame(true);
    }
}
