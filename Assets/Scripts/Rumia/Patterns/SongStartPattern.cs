namespace Rumia {
    /// <summary>
    /// Starts playing the song
    /// </summary>
    public class SongStartPattern : Pattern {
        // ReSharper disable once UnusedMember.Global
        [RumiaAction]
        public void StartSong() {
            GameController.Instance.SongController.PlaySong(GameController.Instance.TimingController.NumberOfMeasuresToSkipOnStart);
        }
    }
}