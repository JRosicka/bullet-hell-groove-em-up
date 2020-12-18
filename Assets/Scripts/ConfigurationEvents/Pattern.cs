using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An event that we can schedule to perform via a PatternMeasure.
/// Has defined behaviors for its collection of PatternActions. 
/// </summary>
public abstract class Pattern : MonoBehaviour {
    [SerializeField]
    private List<PatternAction> PatternActions;

    [PatternActionAttribute]
    public void Spawn(Vector2 position) {
        Transform spawner = GameController.Instance.EnemyManager.transform;
        Instantiate(this, position, Quaternion.identity, spawner);
    }
    
    public void InvokePatternAction(int id) {
        PatternActions.First(e => e.ID == id).InvokePatternAction();
    }

    public PatternAction[] GetAllPatternActions() {
        List<PatternAction> allPatternActions = new List<PatternAction>(PatternActions); 
        return allPatternActions.ToArray();
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(Pattern), true)]
    public class PatternEditor : Editor {
        public bool dirtied;

        public override void OnInspectorGUI() {
            // Draw the default inspector
            DrawDefaultInspector();
            Pattern pattern = target as Pattern;
            if (pattern == null) {
                Debug.LogError("PatternEditor.OnInspectorGUI(): Attempting to inspect a null item");
                return;
            }

            if (GUILayout.Button("Set pattern actions")) {
                SetPatternActions(pattern);
            }
        }

        private void SetPatternActions(Pattern pattern) {
            dirtied = false;

            pattern.PatternActions.Clear();
            
            // Add the null PatternAction if it is not present
            if (pattern.PatternActions.Count == 0) {
                pattern.PatternActions.Add(PatternAction.CreateNullPatternAction());
                // MarkDirty();
            }
            
            // Do inspector GUI of our list of PatternActions
            // ListGUI("PatternActions", ref pattern.PatternActions);
            
            GeneratePatternActions(pattern);

            if (dirtied) {
                // Synchronize the IDs in case any entries were added/removed/reordered
                for (int i = 0; i < pattern.PatternActions.Count; i++) {
                    pattern.PatternActions[i].ID = i;
                }
                EditorUtility.SetDirty(target);
            }
        }

        private void ListGUI(string label, ref List<PatternAction> patternActions) {
            Color defColor = GUI.color;

            GUILayout.BeginVertical(GUILayout.Width(250));

            // Button that populates PatternActions
            GUILayout.Label(GUIContent.none);
            GUI.color = Color.cyan;
            
            GUILayout.BeginHorizontal();
            foreach (PatternAction.PatternActionType patternActionType in (PatternAction.PatternActionType[])Enum.GetValues(typeof(PatternAction.PatternActionType))) {
                // Do not allow the null PatternAction to be created
                if (patternActionType.Equals(PatternAction.PatternActionType.None))
                    continue;
                
                if (GUILayout.Button(patternActionType.ToString(), GUILayout.Width(100)))
                    ListHandleAdd(patternActionType, ref patternActions);
            }
            GUILayout.EndHorizontal();

            GUI.color = defColor;

            // Section where our PatternActions are displayed
            GUILayout.Label(label);

            EditorGUIUtility.labelWidth = 70;
            EditorGUIUtility.fieldWidth = 200;

            int numObjects = patternActions.Count;
            for (int i = 0; i < numObjects; i++) {
                GUI.color = Color.green;
                GUILayout.Label($"{i}. Type: {patternActions[i].type.ToString()}. Name: {patternActions[i].ActionName}");
                GUI.color = defColor;

                // Draw custom GUI for each of the "polymorphic" PatternAction types
                PatternAction act = patternActions[i];
                PatternAction newAct = OnPatternActionGUI(act, i);
                if (newAct != null) {
                    patternActions[i] = newAct;
                    MarkDirty();
                }
            }
            // Set the label width back to the default value
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            GUILayout.EndVertical();
        }

        private PatternAction OnPatternActionGUI(PatternAction patternAction, int listIndex) {
            if (patternAction == null)
                return null;

            PatternAction ret = null;
            
            // Handle the element GUI if that applies
            if (patternAction.type != PatternAction.PatternActionType.None)
                ret = OnPatternActionSubGUI(patternAction, listIndex);
            else
                GUILayout.Label(GUIContent.none);

            return ret;
        }

        private PatternAction OnPatternActionSubGUI(PatternAction patternAction, int listIndex) {
            bool changes = false;

            GUILayout.BeginHorizontal();
            
            string propertyPath = $"PatternActions.Array.data[{listIndex}].{patternAction.type}.OnPatternAction";
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyPath), true, GUILayout.Width(400));
            serializedObject.ApplyModifiedProperties();

            string methodNamePath = $"{propertyPath}.m_PersistentCalls.m_Calls.Array.data[0].m_MethodName";
            SerializedProperty methodNameProperty = serializedObject.FindProperty(methodNamePath);
            if (methodNameProperty != null) {
                string newName = methodNameProperty.stringValue;
                if (newName != patternAction.ActionName) {
                    patternAction.ActionName = newName;
                    changes = true;
                    MarkDirty();
                }
            }

            switch (patternAction.type) {
                case PatternAction.PatternActionType.Basic:
                    break;
                case PatternAction.PatternActionType.Vector:
                    Vector2 newVector = EditorGUILayout.Vector2Field("Vector2", patternAction.Vector.Vector);
                    if (newVector != patternAction.Vector.Vector) {
                        patternAction.Vector.Vector = newVector;
                        changes = true;
                        MarkDirty();
                    }
                    break;
            }
            
            GUILayout.EndHorizontal();

            if (changes) {
                return patternAction;
            } else
                return null;
        }

        private void ListHandleAdd(PatternAction.PatternActionType patternActionType, ref List<PatternAction> patternActions) {
            // Add a new element
            patternActions.Add(PatternAction.CreatePatternAction(patternActionType));
            MarkDirty();
        }

        private void MarkDirty() {
            dirtied = true;
        }

        private void GeneratePatternActions(Pattern pattern) {
            MethodInfo[] methods = pattern.GetType()
                .GetMethods()
                .Where(t => t.GetCustomAttributes(typeof(PatternActionAttribute), false).Length > 0)
                .ToArray();
            foreach (MethodInfo method in methods) {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length >= 2) {
                    Debug.LogError("Too many parameters for method " + method.Name + ". Methods with the [PatternActionAttribute] can only have one parameter.");
                    continue;
                }

                Type subPatternActionType;
                if (parameters.Length == 0) {
                    // No parameters, so use a BasicSubPatternAction
                    subPatternActionType = typeof(PatternAction.BasicSubPatternAction);
                } else {
                    Type parameterType = parameters[0].ParameterType;
                    subPatternActionType = PatternAction.GetPatternActionType(parameterType);
                    if (subPatternActionType == null) {
                        Debug.LogError("No SubPatternAction type assigned to typesDict for parameter type: " + parameterType);
                        continue;
                    }
                }

                PatternAction.PatternActionType newType;
                PatternAction newPatternAction;
                if (subPatternActionType == typeof(PatternAction.BasicSubPatternAction)) {
                    newType = PatternAction.PatternActionType.Basic;
                    newPatternAction = PatternAction.CreatePatternAction(newType);
                    UnityEvent newEvent = new UnityEvent();

                    UnityAction action = (UnityAction) method.CreateDelegate(typeof(UnityAction), pattern);

                    // newEvent.AddListener(action);
                    
                    UnityEventTools.AddPersistentListener(newEvent, action);
                    
                    newPatternAction.Basic.OnPatternAction = newEvent;
                } else if (subPatternActionType == typeof(PatternAction.VectorSubPatternAction)) {
                    newType = PatternAction.PatternActionType.Vector;
                    newPatternAction = PatternAction.CreatePatternAction(newType);
                    PatternAction.VectorEvent newEvent = new PatternAction.VectorEvent();
                    
                    UnityAction<Vector2> action = (UnityAction<Vector2>)method.CreateDelegate(typeof(UnityAction<Vector2>), pattern);

                    UnityEventTools.AddPersistentListener(newEvent, action);
                    
                    newPatternAction.Vector.OnPatternAction = newEvent;
                } else {
                    newType = PatternAction.PatternActionType.None;
                    newPatternAction = PatternAction.CreatePatternAction(newType);
                }

                newPatternAction.ActionName = method.Name;

                pattern.PatternActions.Add(newPatternAction);
                Debug.Log("Found method with PatternActionAttribute: " + method.Name + ". Matching subPatternActionType: " + subPatternActionType);
            }
            
            // Synchronize the IDs in case any entries were added/removed/reordered
            for (int i = 0; i < pattern.PatternActions.Count; i++) {
                pattern.PatternActions[i].ID = i;
            }
            EditorUtility.SetDirty(target);

        }
    }
#endif
}