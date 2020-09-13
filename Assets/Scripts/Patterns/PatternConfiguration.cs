using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Configuration for a bullet pattern
/// </summary>
[CreateAssetMenu(fileName = "PatternConfiguration", menuName = "Resources/Patterns/PatternConfiguration", order = 3)]
public class PatternConfiguration : ScriptableObject {
    public Pattern Pattern;
    
    // Amount of measures to delay before starting the pattern
    public int StartMeasure;
    
    // Measures. It's hard to transcribe sheet music to scriptable objects, okay?
    [Header("Measures list")] 
    public List<PatternMeasureList> MeasuresList;
    
#if UNITY_EDITOR
    [CustomEditor(typeof(PatternConfiguration))]
    public class PatternConfigurationEditor : Editor {
        public override void OnInspectorGUI() {
            // Draw the default inspector
            DrawDefaultInspector();
            PatternConfiguration config = target as PatternConfiguration;
            Pattern pattern = config.Pattern;
            if (!pattern)
                return;

            foreach (PatternMeasureList measures in config.MeasuresList) {
                foreach (PatternMeasure measure in measures.Measures)
                    measure.SetPattern(pattern);
            }
        }
    }
#endif
}

[Serializable]
public class PatternMeasureList {
    public List<PatternMeasure> Measures;
}