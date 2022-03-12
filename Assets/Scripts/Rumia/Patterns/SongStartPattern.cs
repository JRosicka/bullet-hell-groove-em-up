using System.Collections.Generic;

namespace Rumia {
    /// <summary>
    /// Starts playing the song
    /// </summary>
    public class SongStartPattern : Pattern {
        // ReSharper disable once UnusedMember.Global
        [RumiaAction]
        public void StartSong() {
            GameController.Instance.SongController.PlaySong(GameController.Instance.TimingController.NumberOfMeasuresToSkipOnStart);
            GameController.Instance.ProgressBar.SetUpProgressBar(new List<float> {
                7f, 47f, 73f, 86f, 113f, 139f//, 189f, 214f, 285f Ending early at end of section 5
            });
        }
    }
}