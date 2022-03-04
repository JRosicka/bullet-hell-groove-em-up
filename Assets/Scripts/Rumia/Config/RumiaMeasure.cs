using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor;
#if UNITY_EDITOR
using UnityEditor.Callbacks;
#endif

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
        [HideInInspector] [FormerlySerializedAs("PatternActionLists")]
        public RumiaActionList[] RumiaActionLists = new RumiaActionList[SIZE];
        // Collection of values to pass into the RumiaActions' parameters, serialized as strings in order to allow for different types
        [HideInInspector]
        public ChoiceParameterList[] ChoiceParameterLists = new ChoiceParameterList[SIZE];

        public Pattern Pattern;

        private void OnValidate() {
            if (RumiaActionLists.Length != SIZE) {
                Debug.LogWarning("Don't change the size of 'Notes'!");
                Array.Resize(ref RumiaActionLists, SIZE);
            }
        }

#if UNITY_EDITOR
        public void SetPattern(Pattern p) {
            Pattern = p;
        }
        
        /// <summary>
        /// Update <see cref="RumiaAction"/> availability and indices in each <see cref="RumiaMeasure"/> asset
        /// </summary>
        [DidReloadScripts]
        private static void OnScriptsReloaded() {
            foreach (RumiaMeasure measure in ResourcesUtil.GetAllScriptableObjectInstances<RumiaMeasure>()) {
                measure.UpdateRumiaActions();
            }
        } 
        
        private void UpdateRumiaActions() {
            Pattern.GenerateRumiaActions();
            
            // Get all of the RumiaAction names
            List<string> choiceNames = Pattern.GetAllRumiaActions().Select(e => e.ActionName).ToList();

            for (int i = 0; i < SIZE; i++) {
                if (RumiaActionLists[i] == null)
                    RumiaActionLists[i] = new RumiaActionList();
                
                List<RumiaAction> rumiaActions = RumiaActionLists[i].RumiaActions;

                // Handle each individual RumiaAction
                for (int j = 0; j < rumiaActions.Count; j++) {
                    string currentName = rumiaActions[j].ActionName;

                    // If the currently chosen RumiaAction no longer exists, replace it with a NullAction
                    if (!choiceNames.Contains(currentName))
                        rumiaActions[j].ActionName = RumiaAction.NoneString;

                    // Get the ID of the currently selected RumiaAction
                    int currentIndex = Pattern.GetAllRumiaActions().First(e => e.ActionName.Equals(currentName)).ID;
                    rumiaActions[j].ID = currentIndex;
                    
                    Type parameterType = rumiaActions[j].GetSubRumiaAction()?.GetParameterType();
                    Type currentType = rumiaActions[j].GetSubRumiaAction()?.GetParameterType();
                    
                    // If we changed the parameter type, then set the choice parameter value back to null to avoid type weirdness
                    if (parameterType != null && parameterType != currentType)
                        ChoiceParameterLists[i].ChoiceParameters[j] = null;
                }
            }
            
            // Save the changes back to the object
            EditorUtility.SetDirty(this);
        }

        [CustomEditor(typeof(RumiaMeasure))]
        public class RumiaMeasureEditor : Editor {
            public override void OnInspectorGUI() {
                // Draw the default inspector
                DrawDefaultInspector();
                RumiaMeasure measure = target as RumiaMeasure;
                if (measure == null) 
                    return;
                Pattern pattern = measure.Pattern;
                if (!pattern)
                    return;
                
                List<RumiaAction> choices = pattern.GetAllRumiaActions();

                EditorGUILayout.LabelField("32nd note triggers", EditorStyles.boldLabel);
                EditorGUIUtility.labelWidth = 80;
                EditorGUIUtility.fieldWidth = 150;
                Color defColor = GUI.color;
                
                // Handle each group of RumiaActions to be scheduled for this instant in time
                for (int i = 0; i < SIZE; i++) {
                    if (measure.RumiaActionLists[i] == null)
                        measure.RumiaActionLists[i] = new RumiaActionList();

                    EditorGUILayout.BeginHorizontal();
                    List<RumiaAction> rumiaActionList = measure.RumiaActionLists[i].RumiaActions;
                    List<string> choiceParameterList = measure.ChoiceParameterLists[i].ChoiceParameters;
                    
                    // Button to add a new RumiaAction
                    GUI.color = Color.green;
                    if (GUILayout.Button("+", GUILayout.Width(30))) {
                        rumiaActionList.Add(new RumiaAction());
                        choiceParameterList.Add(null);
                    }

                    // Button to remove the last RumiaAction in the list
                    GUI.color = Color.red;
                    if (GUILayout.Button("-", GUILayout.Width(30)) && rumiaActionList.Count > 0) {
                        rumiaActionList.RemoveAt(rumiaActionList.Count - 1);
                        choiceParameterList.RemoveAt(choiceParameterList.Count - 1);
                    }

                    GUI.color = defColor;
                    GUILayout.Label(GetLabel(i), GUILayout.Width(50));
                    
                    // Handle each individual RumiaAction
                    for (int j = 0; j < rumiaActionList.Count; j++) {
                        // Formatting
                        if (j > 0) {
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label("", GUILayout.Width(117));
                        }

                        // Draw the RumiaAction field
                        RumiaAction updatedRumiaAction = rumiaActionList[j];
                        string updatedChoiceParameter = choiceParameterList[j];
                        DrawRumiaActionField(choices, ref updatedRumiaAction, ref updatedChoiceParameter);
                        rumiaActionList[j] = updatedRumiaAction;
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
            private void DrawRumiaActionField(List<RumiaAction> choices, ref RumiaAction rumiaAction, ref string choiceParameter) {
                // Get the name of the currently selected RumiaAction
                string currentName = rumiaAction.ActionName;
                Type currentType = rumiaAction.GetSubRumiaAction()?.GetParameterType();
                    
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
                Type parameterType = rumiaAction.GetSubRumiaAction()?.GetParameterType();
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
                    case RumiaAction.RumiaActionType.Vector:
                        choiceParameter = RumiaAction.VectorSubRumiaAction.SerializeParameter(
                            EditorGUILayout.Vector2Field("Parameter:", RumiaAction.VectorSubRumiaAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.RumiaActionType.Bool:
                        choiceParameter = RumiaAction.BoolSubRumiaAction.SerializeParameter(
                            EditorGUILayout.Toggle("Parameter:", RumiaAction.BoolSubRumiaAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.RumiaActionType.Int:
                        choiceParameter = RumiaAction.IntSubRumiaAction.SerializeParameter(
                            EditorGUILayout.IntField("Parameter:", RumiaAction.IntSubRumiaAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.RumiaActionType.Float:
                        choiceParameter = RumiaAction.FloatSubRumiaAction.SerializeParameter(
                            EditorGUILayout.FloatField("Parameter:", RumiaAction.FloatSubRumiaAction.DeserializeParameter(choiceParameter)));
                        break;
                    case RumiaAction.RumiaActionType.String:
                        choiceParameter = RumiaAction.StringSubRumiaAction.SerializeParameter(
                            EditorGUILayout.TextField("Parameter:", RumiaAction.StringSubRumiaAction.DeserializeParameter(choiceParameter)));
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
        public List<RumiaAction> RumiaActions = new List<RumiaAction>();
    }

    [Serializable]
    public class ChoiceParameterList {
        public List<string> ChoiceParameters = new List<string>();
    }
}
