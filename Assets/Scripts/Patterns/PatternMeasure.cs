using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single measure for a pattern
/// TODO: Force list size to 32nd notes. Also, like, make a better interface to configure stuff like this.
/// </summary>
[CreateAssetMenu(fileName = "PatternMeasure", menuName = "Resources/Patterns/PatternMeasure", order = 3)]
public class PatternMeasure : ScriptableObject {
    // List of 32nd notes. It's hard to transcribe sheet music to scriptable objects, okay?
    [Header("32nd note triggers")]
    public List<bool> Notes;

    public Shot Shot;
}
