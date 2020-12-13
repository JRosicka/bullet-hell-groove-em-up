using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// An event that we can schedule to perform via a PatternMeasure.
/// Has defined behaviors for its collection of PatternActions. 
/// </summary>
public /*abstract*/ class Pattern : MonoBehaviour {
    public enum PatternActionType {
        None,
        Basic,
        Vector
    }

    // TODO apparently this isn't necessary in Unity 2020.1 since you can serialize generics there
    [Serializable]
    public class VectorEvent : UnityEvent<Vector2> { }

    public interface IPatternAction {
        void InvokePatternAction();
    }

    [Serializable]
    public class BasicPatternAction : IPatternAction {
        public UnityEvent OnPatternAction;

        public void InvokePatternAction() {
            OnPatternAction.Invoke();
        }
    }

    [Serializable]
    public class VectorPatternAction : IPatternAction {
        public Vector2 Vector;
        public VectorEvent OnPatternAction;
        public void InvokePatternAction() {
            OnPatternAction.Invoke(Vector);
        }
    }
    
    [Serializable]
    public class PatternAction /*: ScriptableObject*/ {
        public PatternActionType type = PatternActionType.None;
        
        [HideInInspector]
        public int ID;
        public string ActionName;

        // Name must match its enum type. TODO enforce this
        public BasicPatternAction Basic;
        public VectorPatternAction Vector;
        // TODO: More PatternAction types go here
        
        public void InvokePatternAction() {
            switch (type) {
                case PatternActionType.Basic:
                    Basic.InvokePatternAction();
                    break;
                case PatternActionType.Vector:
                    Vector.InvokePatternAction();
                    break;
                case PatternActionType.None:
                    throw new Exception("Somehow, someway, we're attempting to invoke a base PatternAction :(");
            }
        }

        public static PatternAction CreatePatternAction(PatternActionType patternActionType) {
            switch (patternActionType) {
                case PatternActionType.Basic:
                    return CreateBasicPatternAction();
                case PatternActionType.Vector:
                    return CreateVectorPatternAction();
                case PatternActionType.None:
                    return CreateNullPatternAction();
                default:
                    throw new Exception("Failed to create PatternAction for type: " + patternActionType);
            }
        }
        
        private static PatternAction CreateBasicPatternAction() {
            PatternAction act = new PatternAction();
            act.type = PatternActionType.Basic;
            act.Basic = new BasicPatternAction();
            
            return act;
        }

        private static PatternAction CreateVectorPatternAction() {
            PatternAction act = new PatternAction();
            act.type = PatternActionType.Vector;
            act.Vector = new VectorPatternAction();

            return act;
        }

        public static PatternAction CreateNullPatternAction() {
            PatternAction act = new PatternAction();
            return act;
        }
    }
    
    public static readonly string NoneString = "None";
    private static BasicPatternAction NoneAction;

    // static Pattern() {
    //     if (NoneAction == null)
    //         NoneAction = new BasicPatternAction();//ScriptableObject.CreateInstance<BasicPatternAction>();
    //     NoneAction.ActionName = NoneString;
    //     NoneAction.OnPatternAction = null;
    // }
    
    public static readonly string SpawnString = "Spawn";
    // private VectorPatternAction SpawnAction = new VectorPatternAction {
    //     ActionName = SpawnString, OnPatternAction  = new UnityEvent().AddListener(delegate { SpawnPattern(Vector); })
    // };
    
    [SerializeField]
    private List<PatternAction> PatternActions;

    public void Spawn(Vector2 position) {
        Transform spawner = GameController.Instance.EnemyManager.transform;
        Instantiate(this, position, Quaternion.identity, spawner);
    }
    
    public void InvokePatternAction(int id) {
        PatternActions.First(e => e.ID == id).InvokePatternAction();
    }

    public PatternAction[] GetAllPatternActions() {
        List<PatternAction> allPatternActions = new List<PatternAction>(PatternActions); 
        // allPatternActions.Insert(0, NoneAction);
        // allPatternActions.Insert(0, SpawnAction);
        return allPatternActions.ToArray();
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(Pattern), true)]
    public class PatternEditor : Editor {
        public bool dirtied;

        public override void OnInspectorGUI() {
            // Draw the default inspector
            DrawDefaultInspector();
            // serializedObject.Update();
            Pattern pattern = target as Pattern;
            if (pattern == null) {
                Debug.LogError("PatternEditor.OnInspectorGUI(): Attempting to inspect a null item");
                return;
            }

            dirtied = false;

            // Add the null PatternAction if it is not present
            if (pattern.PatternActions.Count == 0) {
                pattern.PatternActions.Add(PatternAction.CreateNullPatternAction());
                MarkDirty();
            }
            
            // Do inspector GUI of our list of PatternActions
            int ret = ListGUI("PatternActions", ref pattern.PatternActions);

            // Handle addition of elements
            // if (ret >= 0)
            //     ListHandleAdd(ref pattern.PatternActions);

            // // TODO: Maybe I would need to add some buttons here for "add BasicPatternAction" and "add VectorPatternAction"
            // if (GUILayout.Button("Add basic pattern action"))
            //     pattern.PatternActions.Add(new BasicPatternAction());
            // if (GUILayout.Button("Add vector pattern action"))
            //     pattern.PatternActions.Add(new VectorPatternAction());

            // int i = 0;
            // List<PatternAction> patternActions = pattern.PatternActions;
            // foreach (PatternAction patternAction in patternActions) {
            //     if (patternAction == null)
            //         continue;
            //
            //     Type type = patternAction.GetType();
            //     PropertyInfo infor = type.GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
            //     if (infor != null)
            //     {
            //         infor.SetValue(patternAction, InspectorMode.Normal, null);
            //     }
            //     SerializedProperty iterator = ((SerializedObject)patternAction).GetIterator();
            //
            //
            //     patternAction.ActionName = EditorGUILayout.TextArea(patternAction.ActionName ?? "");
            //     if (patternAction is BasicPatternAction) {
            //         SerializedProperty sProp = 
            //     }
            //     patternAction.ID = i++;
            // }

            if (dirtied) {
                // Synchronize the IDs in case any entries were added/removed/reordered
                for (int i = 0; i < pattern.PatternActions.Count; i++) {
                    pattern.PatternActions[i].ID = i;
                }
                EditorUtility.SetDirty(target);
            }
        }

        private int ListGUI(string label, ref List<PatternAction> patternActions) {
            int ret = -1;
            Color defColor = GUI.color;

            GUILayout.BeginVertical(GUILayout.Width(250));

            // Button that populates PatternActions
            GUILayout.Label("");
            GUI.color = Color.cyan;
            
            GUILayout.BeginHorizontal();
            foreach (PatternActionType patternActionType in (PatternActionType[])Enum.GetValues(typeof(PatternActionType))) {
                // Do not allow the null PatternAction to be created
                if (patternActionType.Equals(PatternActionType.None))
                    continue;
                
                if (GUILayout.Button(patternActionType.ToString(), GUILayout.Width(100)))
                    ListHandleAdd(patternActionType, ref patternActions);
            }
            GUILayout.EndHorizontal();

            GUI.color = defColor;

            // Section where our PatternActions are displayed
            GUILayout.Label(label);

            EditorGUIUtility.labelWidth = 50;
            EditorGUIUtility.fieldWidth = 200;

            int numObjects = patternActions.Count;
            for (int i = 0; i < numObjects; i++) {
                GUI.color = Color.green;
                GUILayout.Label($"{i}: {patternActions[i].type.ToString()}", GUILayout.Width(60));
                GUI.color = defColor;

                GUILayout.BeginHorizontal();
                {
                    // Draw custom GUI for each of the "polymorphic" PatternAction types
                    PatternAction act = patternActions[i];
                    PatternAction newAct = OnPatternActionGUI(act, i);
                    if (newAct != null) {
                        patternActions[i] = newAct;
                        MarkDirty();
                    }
                }
                GUILayout.EndHorizontal();
            }
            // Set the label width back to the default value
            EditorGUIUtility.labelWidth = 0;
            EditorGUIUtility.fieldWidth = 0;

            GUILayout.EndVertical();

            return ret;
        }

        private PatternAction OnPatternActionGUI(PatternAction patternAction, int listIndex) {
            if (patternAction == null)
                return patternAction;

            GUILayout.BeginHorizontal();
            PatternAction ret = null;
            
            // Handle the element GUI if that applies
            if (patternAction.type != PatternActionType.None)
                ret = OnPatternActionSubGUI(patternAction, listIndex);
            else
                GUILayout.Label("");

            GUILayout.EndHorizontal();
            return ret;
        }

        private PatternAction OnPatternActionSubGUI(PatternAction patternAction, int listIndex) {
            bool changes = false;
            
            GUILayout.Label("Name", GUILayout.Width(40));
            string newName = GUILayout.TextField(patternAction.ActionName, GUILayout.Width(100));
            if (newName != patternAction.ActionName) {
                patternAction.ActionName = newName;
                changes = true;
                MarkDirty();
            }

            string propertyPath = $"PatternActions.Array.data[{listIndex}].{patternAction.type}.OnPatternAction";
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyPath), true, GUILayout.Width(400));
            serializedObject.ApplyModifiedProperties();

            switch (patternAction.type) {
                case PatternActionType.Basic:
                    break;
                case PatternActionType.Vector:
                    Vector2 newVector = EditorGUILayout.Vector2Field("Vector2", patternAction.Vector.Vector);
                    if (newVector != patternAction.Vector.Vector) {
                        patternAction.Vector.Vector = newVector;
                        changes = true;
                        MarkDirty();
                    }
                    break;
            }

            if (changes) {
                return patternAction;
            } else
                return null;
        }

        private void ListHandleAdd(PatternActionType patternActionType, ref List<PatternAction> patternActions) {
            // Add a new element
            patternActions.Add(PatternAction.CreatePatternAction(patternActionType));
            MarkDirty();
        }

        private void MarkDirty() {
            dirtied = true;
        }
        
    }
#endif
}