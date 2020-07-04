using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Single measure for a pattern
/// TODO: like, make a better interface to configure stuff like this.
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
    public string[] NoteActions = new string[SIZE];
    private int[] choiceIndices = new int[SIZE];

    public Shot Shot;

    private void OnValidate() {
        if (NoteActions.Length != SIZE) {
            Debug.LogWarning("Don't change the size of 'Notes'!");
            Array.Resize(ref NoteActions, SIZE);
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(PatternMeasure))]
    public class PatternMeasureEditor : Editor {

        public override void OnInspectorGUI() {
            // Draw the default inspector
            DrawDefaultInspector();
            var measure = target as PatternMeasure;
            string[] choices = measure.Shot.GetValues();

            EditorGUILayout.LabelField("32nd note triggers", EditorStyles.boldLabel);
            for (int i = 0; i < SIZE; i++) {
                measure.choiceIndices[i] = EditorGUILayout.Popup(getLabel(i), measure.choiceIndices[i], choices);
                // Update the selected choice in the underlying object
                measure.NoteActions[i] = choices[measure.choiceIndices[i]];

                if ((i + 1) % ELEMENTS_PER_BEAT == 0)
                    EditorGUILayout.Space();
            }
            // Save the changes back to the object
            EditorUtility.SetDirty(target);
        }

        private string getLabel(int noteIndex) {
            return ((noteIndex / ELEMENTS_PER_BEAT) + 1).ToString() + " " + ((noteIndex % ELEMENTS_PER_BEAT) + 1).ToString() + "/8";
        }
    }
#endif
}
