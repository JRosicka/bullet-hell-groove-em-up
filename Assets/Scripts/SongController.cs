using UnityEngine;

/// <summary>
/// Handler of song playback
/// </summary>
public class SongController : MonoBehaviour {
    // Sound that's currently playing. Only one at a time, lads.
    private AudioSource currentlyPlayingBackground;
    public bool soundsToggled = true;
    public AudioSource Music;

    void Start() {
        PlaySong(Music);
    }

    private void PlaySong(AudioSource sound) {
        if (!soundsToggled)
            return;

        if (currentlyPlayingBackground)
            currentlyPlayingBackground.Stop();

        if (sound == null)
            return;

        currentlyPlayingBackground = sound;
        sound.loop = false;
        sound.Play();
    }
}
