using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rumia {
    /// <summary>
    /// Single measure for a pattern
    /// </summary>
    [CreateAssetMenu(fileName = "RumiaMeasure", menuName = "Resources/Patterns/RumiaMeasure", order = 3)]
    public class RumiaMeasure : ScriptableObject {
        private const int SIZE = 32;
        private const int ELEMENTS_PER_BEAT = 8;

        // List of actions to do for every 32nd note. It's hard to transcribe sheet music to scriptable objects, okay?
        // TODO: Hey this looks kinda neat https://stackoverflow.com/questions/60864308/how-to-make-an-enum-like-unity-inspector-drop-down-menu-from-a-string-array-with
        // TODO: Functionality for updating a shot that isn't necessarily the most recently fired one, maybe. Would require using ordered pairs (index, enum string) rather than just enum strings. 
        // Would also probably want to use a list per element instead of a single value since we'd want to be able to do multiple actions at once from a single RumiaMeasure
        [HideInInspector]
        public RumiaActionList[] PatternActionLists = new RumiaActionList[SIZE];
        // Collection of values to pass into the RumiaActions' parameters, serialized as strings in order to allow for different types
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
        [CustomEditor(typeof(RumiaMeasure))]
        public class RumiaMeasureEditor : Editor {
            private bool dirtied = true;

            public override void OnInspectorGUI() {
                // Draw the default inspector
                DrawDefaultInspector();
                RumiaMeasure measure = target as RumiaMeasure;
                if (measure == null) 
                    return;
                Pattern pattern = measure.Pattern;
                if (!pattern)
                    return;

                // The only way this would generate a different result from last time is if the assembly was recompiled, 
                // which would result in this dirtied flag getting reset to its default value (true). If this has happened, 
                // regenerate the RumiaActions for this RumiaMeasure's pattern
                if (dirtied) {
                    pattern.GeneratePatternActions();
                    dirtied = false;
                }
                
                List<RumiaAction> choices = pattern.GetAllPatternActions();

                EditorGUILayout.LabelField("32nd note triggers", EditorStyles.boldLabel);
                EditorGUIUtility.labelWidth = 80;
                EditorGUIUtility.fieldWidth = 150;
                Color defColor = GUI.color;
                
                // Handle each group of RumiaActions to be scheduled for this instant in time
                for (int i = 0; i < SIZE; i++) {
                    if (measure.PatternActionLists[i] == null)
                        measure.PatternActionLists[i] = new RumiaActionList();

                    EditorGUILayout.BeginHorizontal();
                    List<RumiaAction> patternActionList = measure.PatternActionLists[i].RumiaActions;
                    List<string> choiceParameterList = measure.ChoiceParameterLists[i].ChoiceParameters;
                    
                    // Button to add a new RumiaAction
                    GUI.color = Color.green;
                    if (GUILayout.Button("+", GUILayout.Width(30))) {
                        patternActionList.Add(new RumiaAction());
                        choiceParameterList.Add(null);
                    }

                    // Button to remove the last RumiaAction in the list
                    GUI.color = Color.red;
                    if (GUILayout.Button("-", GUILayout.Width(30)) && patternActionList.Count > 0) {
                        patternActionList.RemoveAt(patternActionList.Count - 1);
                        choiceParameterList.RemoveAt(choiceParameterList.Count - 1);
                    }

                    GUI.color = defColor;
                    GUILayout.Label(GetLabel(i), GUILayout.Width(50));
                    
                    // Handle each individual RumiaAction
                    for (int j = 0; j < patternActionList.Count; j++) {
                        // Formatting
                        if (j > 0) {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(117));
                        }

                        // Draw the RumiaAction field
                        RumiaAction updatedRumiaAction = patternActionList[j];
                        string updatedChoiceParameter = choiceParameterList[j];
                        DrawPatternActionField(choices, ref updatedRumiaAction, ref updatedChoiceParameter);
                        patternActionList[j] = updatedRumiaAction;
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
            /// Display the choices and any configurable parameters for this pattern's available RumiaActions, and update
            /// with any changes we make
            /// </summary>
            private void DrawPatternActionField(List<RumiaAction> choices, ref RumiaAction rumiaAction, ref string choiceParameter) {
                // Get the name of the currently selected RumiaAction
                string currentName = rumiaAction.ActionName;
                Type currentType = rumiaAction.GetSubPatternAction()?.GetParameterType();
                    
                // Get all of the RumiaAction names
                List<string> choiceNames = choices.Select(e => e.ActionName).ToList();
                    
                // If the currently chosen RumiaAction no longer exists, replace it with a NullAction
                if (!choiceNames.Contains(currentName))
                    currentName = RumiaAction.NoneString;
                    
                // Get the ID of the currently selected RumiaAction
                int currentIndex = choices.First(e => e.ActionName.Equals(currentName)).ID;
                    
                // Present the RumiaActions by a list of names and allow us to select a new one
                int chosenIndex = EditorGUILayout.Popup(currentIndex, 
                        choices.Select(e => e.ActionName).ToArray());
                
                // Update the selected choice
                rumiaAction = choices[chosenIndex];

                // Handle presenting and updating the parameter value for the selected RumiaAction
                Type parameterType = rumiaAction.GetSubPatternAction()?.GetParameterType();
                if (parameterType != null) {
                    // If we changed the parameter type, then set the choice parameter value back to null to avoid type weirdness
                    if (parameterType != currentType)
                        choiceParameter = null;

                    DrawParameterField(rumiaAction, ref choiceParameter);
                }
            }

            /// <summary>
            /// Handles deserializing, displaying, and re-serializing parameter data for the specified RumiaAction
            /// </summary>
            private void DrawParameterField(RumiaAction rumiaAction, ref string choiceParameter) {
                // TODO: Might want to use some sort of JSON Serialization if this gets more complicated. 
                // http://wiki.unity3d.com/index.php/SimpleJSON
                switch (rumiaAction.type) {
                    case RumiaAction.PatternActionType.Vector:
                        choiceParameter = RumiaAction.VectorSubPatternAction.SerializeParameter(
                            EditorGUILayout.Vector2Field("Parameter:", RumiaAction.VectorSubPatternAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.PatternActionType.Bool:
                        choiceParameter = RumiaAction.BoolSubPatternAction.SerializeParameter(
                            EditorGUILayout.Toggle("Parameter:", RumiaAction.BoolSubPatternAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.PatternActionType.Int:
                        choiceParameter = RumiaAction.IntSubPatternAction.SerializeParameter(
                            EditorGUILayout.IntField("Parameter:", RumiaAction.IntSubPatternAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.PatternActionType.Float:
                        choiceParameter = RumiaAction.FloatSubPatternAction.SerializeParameter(
                            EditorGUILayout.FloatField("Parameter:", RumiaAction.FloatSubPatternAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.PatternActionType.String:
                        choiceParameter = RumiaAction.StringSubPatternAction.SerializeParameter(
                            EditorGUILayout.TextField("Parameter:", RumiaAction.StringSubPatternAction.DeserializeParameter(choiceParameter)));
                        break;
                    default:
                        throw new Exception("Did not properly account for rumiaAction of type " + rumiaAction.type);
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
                GUIStyle horizontalLine = new GUIStyle {
                    normal = {background = EditorGUIUtility.whiteTexture},
                    margin = new RectOffset(0, 0, 4, 4),
                    fixedHeight = 1
                };

                Color c = GUI.color;
                GUI.color = Color.grey;
                GUILayout.Box(GUIContent.none, horizontalLine);
                GUI.color = c;
            }
        }
    #endif
    }

    [Serializable]
    public class RumiaActionList {
        [FormerlySerializedAs("PatternActions")] 
        public List<RumiaAction> RumiaActions = new List<RumiaAction>();
    }

    [Serializable]
    public class ChoiceParameterList {
        public List<string> ChoiceParameters = new List<string>();
    }
}
