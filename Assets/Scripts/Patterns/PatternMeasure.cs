using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Single measure for a pattern
/// </summary>
[CreateAssetMenu(fileName = "PatternMeasure", menuName = "Resources/Patterns/PatternMeasure", order = 3)]
public class PatternMeasure : ScriptableObject {
    private const int SIZE = 32;
    private const int ELEMENTS_PER_BEAT = 8;

    // List of actions to do for every 32nd note. It's hard to transcribe sheet music to scriptable objects, okay?
    // TODO: Hey this looks kinda neat https://stackoverflow.com/questions/60864308/how-to-make-an-enum-like-unity-inspector-drop-down-menu-from-a-string-array-with
    // TODO: Functionality for updating a shot that isn't necessarily the most recently fired one, maybe. Would require using ordered pairs (index, enum string) rather than just enum strings. 
    // Would also probably want to use a list per element instead of a single value since we'd want to be able to do multiple actions at once from a single PatternMeasure
    [HideInInspector]
    public PatternAction[] PatternActions = new PatternAction[SIZE];
    // Collection of values to pass into the PatternActions' parameters, serialized as strings in order to allow for different types
    [HideInInspector]
    public string[] choiceParameters = new string[SIZE];

    public Pattern Pattern;

    private void OnValidate() {
        if (PatternActions.Length != SIZE) {
            Debug.LogWarning("Don't change the size of 'Notes'!");
            Array.Resize(ref PatternActions, SIZE);
        }
    }

    public void SetPattern(Pattern p) {
        Pattern = p;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(PatternMeasure))]
    public class PatternMeasureEditor : Editor {
        private bool dirtied = true;

        public override void OnInspectorGUI() {
            // Draw the default inspector
            DrawDefaultInspector();
            PatternMeasure measure = target as PatternMeasure;
            Pattern pattern = measure.Pattern;
            if (!pattern)
                return;

            // The only way this would generate a different result from last time is if the assembly was recompiled, 
            // which would result in this dirtied flag getting reset to its default value (true). If this has happened, 
            // regenerate the PatternActions for this PatternMeasure's pattern
            if (dirtied) {
                Debug.Log("Generating pattern actions for Pattern: " + pattern);
                pattern.GeneratePatternActions();
                dirtied = false;
            }
            
            List<PatternAction> choices = pattern.GetAllPatternActions();

            EditorGUILayout.LabelField("32nd note triggers", EditorStyles.boldLabel);
            EditorGUIUtility.labelWidth = 80;
            EditorGUIUtility.fieldWidth = 150;
            
            // Display the choices and any configurable parameters for this pattern's available PatternActions, and update
            // with any changes we make
            for (int i = 0; i < SIZE; i++) {
                EditorGUILayout.BeginHorizontal();
                DrawPatternActionField(choices, measure, i);
                EditorGUILayout.EndHorizontal();

                if ((i + 1) % ELEMENTS_PER_BEAT == 0)
                    EditorGUILayout.Space();
            }
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;
            
            // Save the changes back to the object
            EditorUtility.SetDirty(target);
        }

        /// <summary>
        /// Display the choices and any configurable parameters for this pattern's available PatternActions, and update
        /// with any changes we make
        /// </summary>
        private void DrawPatternActionField(List<PatternAction> choices, PatternMeasure measure, int index) {
            // Get the name of the currently selected PatternAction
            string currentName = measure.PatternActions[index].ActionName;
                
            // Get all of the PatternAction names
            List<string> choiceNames = choices.Select(e => e.ActionName).ToList();
                
            // If the currently chosen PatternAction no longer exists, replace it with a NullAction
            if (!choiceNames.Contains(currentName))
                currentName = PatternAction.NoneString;
                
            // Get the ID of the currently selected PatternAction
            int currentIndex = choices.First(e => e.ActionName.Equals(currentName)).ID;
                
            // Present the PatternActions by a list of names and allow us to select a new one
            int chosenIndex = EditorGUILayout.Popup(GetLabel(index), currentIndex,
                choices.Select(e => e.ActionName).ToArray());
                
            // Update the selected choice
            measure.PatternActions[index] = choices[chosenIndex];

            // Handle presenting and updating the parameter value for the selected PatternAction
            Type parameterType = measure.PatternActions[index].GetSubPatternAction()?.GetParameterType();
            if (parameterType != null) {
                DrawParameterField(measure, index);
            }
        }

        /// <summary>
        /// Handles deserializing, displaying, and re-serializing parameter data for the specified PatternAction
        /// </summary>
        private void DrawParameterField(PatternMeasure measure, int index) {
            PatternAction patternAction = measure.PatternActions[index];
            string parameter = measure.choiceParameters[index];
            
            // TODO: Might want to use some sort of JSON Serialization if this gets more complicated. 
            // http://wiki.unity3d.com/index.php/SimpleJSON
            switch (patternAction.type) {
                case PatternAction.PatternActionType.Vector:
                    parameter = PatternAction.VectorSubPatternAction.SerializeVector2(
                        EditorGUILayout.Vector2Field("Parameter:", PatternAction.VectorSubPatternAction.DeserializeVector2(parameter)));
                    break;
                default:
                    throw new Exception("Did not properly account for patternAction of type " + patternAction.type);
            }

            measure.choiceParameters[index] = parameter;
        }
        
        
        /// <summary>
        /// Returns the label to display in the inspector for a note
        /// </summary>
        private static string GetLabel(int noteIndex) {
            return (noteIndex / ELEMENTS_PER_BEAT + 1) + " " + (noteIndex % ELEMENTS_PER_BEAT + 1) + "/8";
        }
    }
#endif
}
