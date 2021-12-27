using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Keeps track of and controls transition between <see cref="GameBackground"/>s. 
/// </summary>
public class GameBackgroundController : MonoBehaviour {
    public List<GameBackground> Backgrounds;
    private GameBackground activeBackground;

    public void TransitionToBackground(GameBackground destinationBackground) {
        if (destinationBackground != null && activeBackground == destinationBackground)
            return;

        // Turn off the current background
        activeBackground.Deactivate();
        
        // Turn on the new background
        activeBackground = destinationBackground;
        activeBackground.Activate();
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameBackgroundController))]
    public class GameBackgroundControllerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            GameBackgroundController controller = target as GameBackgroundController;

            if (controller == null || controller.Backgrounds == null)
                return;

            int i = 0;
            foreach (GameBackground background in controller.Backgrounds) {
                if (GUILayout.Button("Transition to BG " + i))
                    controller.TransitionToBackground(background);
                i++;
            }
        }
    }
#endif
}
