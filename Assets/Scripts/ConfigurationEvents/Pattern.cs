using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// An entity that we can schedule operations to be performed on via <see cref="PatternMeasure"/>.
/// This keeps a collection of <see cref="PatternActions"/> that it creates based on its methods tagged with
/// <see cref="PatternActionAttribute"/>. This is for ease of use so that entity-specific behaviors can be defined
/// in child classes to be easily invoked via <see cref="PatternMeasure"/>s.
/// </summary>
public abstract class Pattern : MonoBehaviour {
    [SerializeField, HideInInspector]
    private List<PatternAction> PatternActions;

    /// <summary>
    /// Instantiate this pattern in the scene at a specified position
    /// </summary>
    [PatternActionAttribute]
    public void Spawn(Vector2 position) {
        Transform spawner = GameController.Instance.EnemyManager.transform;
        Instantiate(this, position, Quaternion.identity, spawner);
    }
    
    /// <summary>
    /// Perform the <see cref="PatternAction"/> that was scheduled
    /// </summary>
    /// <param name="id"></param>
    public void InvokePatternAction(int id) {
        PatternActions.First(e => e.ID == id).InvokePatternAction();
    }

    public PatternAction[] GetAllPatternActions() {
        List<PatternAction> allPatternActions = new List<PatternAction>(PatternActions); 
        return allPatternActions.ToArray();
    }
    
    #if UNITY_EDITOR
    
    /// <summary>
    /// Delete whatever data we have in <see cref="PatternActions"/> and recreate it.
    /// - The first PatternAction in the list is a NullPatternAction
    /// - Additional entries are created and added for each method in this script with a
    ///   <see cref="PatternActionAttribute"/> tag.
    ///
    /// Also sets the int IDs of each PatternAction
    /// </summary>
    public void GeneratePatternActions() {
        PatternActions.Clear();
            
        // Add the null PatternAction if it is not present 
        PatternActions.Add(PatternAction.CreateNullPatternAction());

        MethodInfo[] methods = GetType()
            .GetMethods()
            .Where(t => t.GetCustomAttributes(typeof(PatternActionAttribute), false).Length > 0)
            .ToArray();
        foreach (MethodInfo method in methods) {
            GeneratePatternActionForMethod(method);
        }
        
        // Synchronize the IDs in case any entries were added/removed/reordered
        for (int i = 0; i < PatternActions.Count; i++) {
            PatternActions[i].ID = i;
        }
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Create a pattern action for a single method. This determines what type of SubPatternAction to use based on the
    /// type of the method's parameter. Only accepts methods with a single parameter or no parameters.
    /// Creates the new PatternAction and adds it to <see cref="PatternActions"/>.
    /// </summary>
    private void GeneratePatternActionForMethod(MethodInfo method) {
        ParameterInfo[] parameters = method.GetParameters();
        
        // Only accept methods with zero or one parameter
        if (parameters.Length >= 2) {
            Debug.LogError("Too many parameters for method " + method.Name + ". Methods with the [PatternActionAttribute] can only have one parameter.");
            return;
        }

        PatternAction.PatternActionType actionType;
        if (parameters.Length == 0) {
            // No parameters, so use a BasicSubPatternAction
            actionType = PatternAction.PatternActionType.Basic;
        } else {
            // Exactly one parameter. Get its mapped PatternAction type.
            Type parameterType = parameters[0].ParameterType;
            actionType = PatternAction.GetPatternActionType(parameterType);
            if (actionType == PatternAction.PatternActionType.Undefined) {
                Debug.LogError("No SubPatternAction type assigned to typesDict for parameter type: " + parameterType);
                return;
            }
        }

        // Create the PatternAction based on the per-SubPatternAction definition
        PatternAction patternAction = PatternAction.CreatePatternAction(actionType);
        patternAction.ActionName = method.Name;
        patternAction.GeneratePatternActionEvent(method, this);
        
        PatternActions.Add(patternAction);
        Debug.Log("Found method with PatternActionAttribute: " + method.Name + ". Matching PatternActionType: " + actionType);
    }

    #region CustomEditor

    /// <summary>
    /// Not actually used for anything right now, but kept around as a reference for displaying PatternActions and stuff
    /// in the inspector
    /// </summary>
    [CustomEditor(typeof(Pattern), true)]
    public class PatternEditor : Editor {
        public bool dirtied;
        
        private void SetPatternActions(Pattern pattern) {
            dirtied = false;

            pattern.PatternActions.Clear();
            
            // Add the null PatternAction if it is not present
            if (pattern.PatternActions.Count == 0) {
                pattern.PatternActions.Add(PatternAction.CreateNullPatternAction());
            }
            
            // Do inspector GUI of our list of PatternActions
            // ListGUI("PatternActions", ref pattern.PatternActions);
            
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
    }
    
    #endregion

#endif
}