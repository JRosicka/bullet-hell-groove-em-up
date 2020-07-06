using UnityEngine;

/// <summary>
/// Configuration for a bullet pattern
/// </summary>
[CreateAssetMenu(fileName = "PatternConfiguration", menuName = "Resources/Patterns/PatternConfiguration", order = 3)]
public class PatternConfiguration : ScriptableObject {
    // Collection of 4-measure groups. It's hard to transcribe sheet music to scriptable objects, okay?
    [Header("4-measure groups")]
    public Pattern4Measures MeasureGroup1;
    public Pattern4Measures MeasureGroup2;
    public Pattern4Measures MeasureGroup3;
    public Pattern4Measures MeasureGroup4;
}

