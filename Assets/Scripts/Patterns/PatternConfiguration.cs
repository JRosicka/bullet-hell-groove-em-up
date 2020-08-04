using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Configuration for a bullet pattern
/// </summary>
[CreateAssetMenu(fileName = "PatternConfiguration", menuName = "Resources/Patterns/PatternConfiguration", order = 3)]
public class PatternConfiguration : ScriptableObject {
    // Amount of measures to delay before starting the pattern
    public int StartMeasure;
    
    // Measures. It's hard to transcribe sheet music to scriptable objects, okay?
    [Header("Measures list")] public List<PatternMeasure> Measures;

}

