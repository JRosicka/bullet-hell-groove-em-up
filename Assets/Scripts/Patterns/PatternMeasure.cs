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
    public PatternActionList[] PatternActionLists = new PatternActionList[SIZE];
    // Collection of values to pass into the PatternActions' parameters, serialized as strings in order to allow for different types
    [HideInInspector]
    public ChoiceParameterList[] ChoiceParameterLists = new ChoiceParameterList[SIZE];

    public Pattern Pattern;

    private void OnValidate() {
        if (PatternActionLists.Length != SIZE) {
            Debug.LogWarning("Don't change the size of 'Notes'!");
            Array.Resize(ref PatternActionLists, SIZE);
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
            Color defColor = GUI.color;
            
            // Handle each group of PatternActions to be scheduled for this instant in time
            for (int i = 0; i < SIZE; i++) {
                if (measure.PatternActionLists[i] == null)
                    measure.PatternActionLists[i] = new PatternActionList();

                EditorGUILayout.BeginHorizontal();
                List<PatternAction> patternActionList = measure.PatternActionLists[i].PatternActions;
                List<string> choiceParameterList = measure.ChoiceParameterLists[i].ChoiceParameters;
                
                // Button to add a new PatternAction
                GUI.color = Color.green;
                if (GUILayout.Button("+", GUILayout.Width(30))) {
                    patternActionList.Add(new PatternAction());
                    choiceParameterList.Add(null);
                }

                // Button to remove the last PatternAction in the list
                GUI.color = Color.red;
                if (GUILayout.Button("-", GUILayout.Width(30)) && patternActionList.Count > 0) {
                    patternActionList.RemoveAt(patternActionList.Count - 1);
                    choiceParameterList.RemoveAt(choiceParameterList.Count - 1);
                }

                GUI.color = defColor;
                GUILayout.Label(GetLabel(i), GUILayout.Width(50));
                
                // Handle each individual PatternAction
                for (int j = 0; j < patternActionList.Count; j++) {
                    // Formatting
                    if (j > 0) {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("", GUILayout.Width(117));
                    }

                    // Draw the PatternAction field
                    PatternAction updatedPatternAction = patternActionList[j];
                    string updatedChoiceParameter = choiceParameterList[j];
                    DrawPatternActionField(choices, ref updatedPatternAction, ref updatedChoiceParameter);
                    patternActionList[j] = updatedPatternAction;
                    choiceParameterList[j] = updatedChoiceParameter;
                }

                EditorGUILayout.EndHorizontal();

                if ((i + 1) % ELEMENTS_PER_BEAT == 0 && i < (SIZE - 1))
                    HorizontalLine();
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
        private void DrawPatternActionField(List<PatternAction> choices, ref PatternAction patternAction, ref string choiceParameter) {
            // Get the name of the currently selected PatternAction
            string currentName = patternAction.ActionName;
            Type currentType = patternAction.GetSubPatternAction()?.GetParameterType();
                
            // Get all of the PatternAction names
            List<string> choiceNames = choices.Select(e => e.ActionName).ToList();
                
            // If the currently chosen PatternAction no longer exists, replace it with a NullAction
            if (!choiceNames.Contains(currentName))
                currentName = PatternAction.NoneString;
                
            // Get the ID of the currently selected PatternAction
            int currentIndex = choices.First(e => e.ActionName.Equals(currentName)).ID;
                
            // Present the PatternActions by a list of names and allow us to select a new one
            int chosenIndex = EditorGUILayout.Popup(currentIndex, 
                    choices.Select(e => e.ActionName).ToArray());
            
            // Update the selected choice
            patternAction = choices[chosenIndex];

            // Handle presenting and updating the parameter value for the selected PatternAction
            Type parameterType = patternAction.GetSubPatternAction()?.GetParameterType();
            if (parameterType != null) {
                // If we changed the parameter type, then set the choice parameter value back to null to avoid type weirdness
                if (parameterType != currentType)
                    choiceParameter = null;

                DrawParameterField(patternAction, ref choiceParameter);
            }
        }

        /// <summary>
        /// Handles deserializing, displaying, and re-serializing parameter data for the specified PatternAction
        /// </summary>
        private void DrawParameterField(PatternAction patternAction, ref string choiceParameter) {
            // TODO: Might want to use some sort of JSON Serialization if this gets more complicated. 
            // http://wiki.unity3d.com/index.php/SimpleJSON
            switch (patternAction.type) {
                case PatternAction.PatternActionType.Vector:
                    choiceParameter = PatternAction.VectorSubPatternAction.SerializeParameter(
                        EditorGUILayout.Vector2Field("Parameter:", PatternAction.VectorSubPatternAction.DeserializeParameter(choiceParameter)));
                    break;
                case PatternAction.PatternActionType.Bool:
                    choiceParameter = PatternAction.BoolSubPatternAction.SerializeParameter(
                        EditorGUILayout.Toggle("Parameter:", PatternAction.BoolSubPatternAction.DeserializeParameter(choiceParameter)));
                    break;
                case PatternAction.PatternActionType.Int:
                    choiceParameter = PatternAction.IntSubPatternAction.SerializeParameter(
                        EditorGUILayout.IntField("Parameter:", PatternAction.IntSubPatternAction.DeserializeParameter(choiceParameter)));
                    break;
                case PatternAction.PatternActionType.Float:
                    choiceParameter = PatternAction.FloatSubPatternAction.SerializeParameter(
                        EditorGUILayout.FloatField("Parameter:", PatternAction.FloatSubPatternAction.DeserializeParameter(choiceParameter)));
                    break;
                case PatternAction.PatternActionType.String:
                    choiceParameter = PatternAction.StringSubPatternAction.SerializeParameter(
                        EditorGUILayout.TextField("Parameter:", PatternAction.StringSubPatternAction.DeserializeParameter(choiceParameter)));
                    break;
                default:
                    throw new Exception("Did not properly account for patternAction of type " + patternAction.type);
            }
        } 
        
        
        /// <summary>
        /// Returns the label to display in the inspector for a note
        /// </summary>
        private static string GetLabel(int noteIndex) {
            return (noteIndex / ELEMENTS_PER_BEAT + 1) + " " + (noteIndex % ELEMENTS_PER_BEAT + 1) + "/8";
        }

        /// <summary>
        /// Makes a slick grey horizontal line in the inspector
        /// </summary>
        private static void HorizontalLine() {
            GUIStyle horizontalLine = new GUIStyle();
            horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            horizontalLine.fixedHeight = 1;

            var c = GUI.color;
            GUI.color = Color.grey;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }
    }
#endif
}

[Serializable]
public class PatternActionList {
    public List<PatternAction> PatternActions = new List<PatternAction>();
}

[Serializable]
public class ChoiceParameterList {
    public List<string> ChoiceParameters = new List<string>();
}