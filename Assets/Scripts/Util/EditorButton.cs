using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class EditorButton : MonoBehaviour {
    public UnityEvent Event;

#if UNITY_EDITOR
    [CustomEditor(typeof(EditorButton))]
    public class EmitterEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            EditorButton button = target as EditorButton;
            if (GUILayout.Button("Do the thing")) {
                button.Event.Invoke();
            }
        }
    }
#endif

}
