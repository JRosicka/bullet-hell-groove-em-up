using System.Collections;
using UnityEngine;

/// <summary>
/// Handler of song and other audio clip playback. Currently only allows one audio clip to play at once.
/// </summary>
public class SongController : MonoBehaviour {
    // Sound that's currently playing. Only one at a time, lads.
    private AudioSource currentlyPlayingBackground;
    public bool soundsToggled = true;
    public AudioSource Music;
    public AudioSource VictorySoundEffect;
    private TimingController timingController;

    void Start() {
        timingController = FindObjectOfType<TimingController>();
    }

    public void PlayVictorySoundEffect() {
        PlaySoundClip(VictorySoundEffect);
    }

    public void PlaySong(int startMeasure) {
        PlaySoundClip(Music, timingController.GetWholeNoteTime() * startMeasure);
    }
    
    private void PlaySoundClip(AudioSource sound, float startTime = 0f) {
        if (!soundsToggled)
            return;

        if (currentlyPlayingBackground)
            currentlyPlayingBackground.Stop();

        if (sound == null)
            return;

        currentlyPlayingBackground = sound;
        sound.loop = false;
        sound.time = startTime;
        sound.Play();
    }
}
