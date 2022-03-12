using UnityEngine;

/// <summary>
/// Git yer timing-related stuff here
/// </summary>
public class TimingController : MonoBehaviour {
    public float BPM;    // TODO: Wooooah pretty awful bug here - over time, the game slows down just a teensy bit, so I currently need to have the BPM here be sliiightly higher (<1) than the actual BPM. This should be fixed, and then I should make this into an int again instead of a float. 
    
    public int NumberOfBeatsToWaitBeforeStarting;
    public int NumberOfMeasuresToSkipOnStart;
    public float DelaySecondsBeforeAllowedToRestart = 2f;

    private float BPS => BPM / 60f;

    public float GetWholeNoteTime() {
        return 4f / BPS;
    }

    public float GetHalfNoteTime() {
        return 4f / (2f * BPS);
    }

    public float GetQuarterNoteTime() {
        return 4f / (4f * BPS);
    }

    public float GetEigthNoteTime() {
        return 4f / (8f * BPS);
    }

    public float GetSixteenthNoteTime() {
        return 4f / (16f * BPS);
    }

    public float GetThirtysecondNoteTime() {
        return 4f / (32f * BPS);
    }

    public float GetStartDelay() {
        return NumberOfBeatsToWaitBeforeStarting * GetQuarterNoteTime();
    }
}
