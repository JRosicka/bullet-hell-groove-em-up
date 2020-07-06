using UnityEngine;

/// <summary>
/// Git yer timing-related stuff here
/// </summary>
public class TimingController : MonoBehaviour {
    public int BPM;
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
}
