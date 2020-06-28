using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternController : MonoBehaviour {
    public Transform Spawner;
    public PatternConfiguration Pattern;

    private GameController gameController;
    private float timeElapsed;

    // TODO: Do we want to get rid of this and instead just put a FireTime value into Shot that is set at instantiation? Probably. 
    private struct TimedShot {
        public float FireTime;
        public Shot Shot;

        public TimedShot(float fireTime, Shot shot) {
            FireTime = fireTime;
            Shot = shot;
        }
    }
    // The original list of configured shots configured at start
    private List<TimedShot> configuredShots;
    // An updated list originally copied from configuredShots
    private List<TimedShot> queuedShots;

    // TODO: We are deciding for now to queue all of the shots when this pattern is instantiated rather than more dynamically determinig when to shoot in the update method. 
    // This is probably fine. 
    private void Start() {
        gameController = FindObjectOfType<GameController>();

        configuredShots = DetermineShotTimings();

        queuedShots = new List<TimedShot>(configuredShots);
    }

    private List<TimedShot> DetermineShotTimings() {
        List<TimedShot> ret = new List<TimedShot>();

        List<Pattern4Measures> measureGroups = new List<Pattern4Measures>();
        measureGroups.Add(Pattern.MeasureGroup1);
        measureGroups.Add(Pattern.MeasureGroup2);
        measureGroups.Add(Pattern.MeasureGroup3);
        measureGroups.Add(Pattern.MeasureGroup4);

        List<PatternMeasure> measures = new List<PatternMeasure>();
        foreach (Pattern4Measures measureGroup in measureGroups) {
            measures.Add(measureGroup.Measure1);
            measures.Add(measureGroup.Measure2);
            measures.Add(measureGroup.Measure3);
            measures.Add(measureGroup.Measure4);
        }

        List<bool> thirtysecondNotes = new List<bool>();
        for (int i = 0; i < measures.Count; i++) {
            for (int j = 0; j < 32; j++) {  // TODO: Magic number
                if (measures[i].Notes[j]) {
                    int elapsedThirtySecondNotes = i * 32 + j;
                    TimedShot timedShot = new TimedShot(gameController.GetThirtysecondNoteTime() * elapsedThirtySecondNotes, measures[i].Shot);
                    ret.Add(timedShot);
                }
            }
        }

        // TODO: Do we need to make sure the return list is ordered by FireTime?
        return ret;
    }

    private void Update() {
        timeElapsed += Time.deltaTime;

        int shotsFired = 0;
        for (int i = 0; i < queuedShots.Count; i++) {
            TimedShot shot = queuedShots[i];
            if (shot.FireTime < timeElapsed) {
                // Shoot your shot.Shot, my dude
                Shoot(shot.Shot);
                shotsFired++;
            } else {
                // Since we assume that the queuedShots list is ordered by FireTime, we know that none of the remaining shots should be fired yet
                break;
            }
        }

        queuedShots.RemoveRange(0, shotsFired);
    }

    private void Shoot(Shot shot) {
        Instantiate(shot, Spawner);
    }
}
