using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 4 measure group for bullet patterns
/// </summary>
[CreateAssetMenu(fileName = "Pattern4Measures", menuName = "Resources/Patterns/Pattern4Measures", order = 3)]
public class Pattern4Measures : ScriptableObject {
    // Collection of measures. It's hard to transcribe sheet music to scriptable objects, okay?
    [Header("Measures")]
    public PatternMeasure Measure1;
    public PatternMeasure Measure2;
    public PatternMeasure Measure3;
    public PatternMeasure Measure4;
}
