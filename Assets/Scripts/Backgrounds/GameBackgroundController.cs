using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Keeps track of and controls transition between <see cref="GameBackground"/>s. 
/// </summary>
public class GameBackgroundController : MonoBehaviour {
    public List<GameBackground> Backgrounds;
    private GameBackground activeBackground;

    public void Start() {
        Backgrounds.ForEach(bg => {
            bg.gameObject.SetActive(true);
            bg.Initialize();
            if (bg.ActiveAtStart) {
                if (activeBackground != null) {
                    throw new InvalidOperationException("Can't have more than one background active at start!");
                }
                bg.Activate(false);
                activeBackground = bg;
            } else {
                bg.Deactivate(false);
            }
        });
    }

    public void TransitionToBackground(GameBackground destinationBackground) {
        if (destinationBackground != null && activeBackground == destinationBackground)
            return;

        // Turn off the current background
        if (activeBackground != null) {
            activeBackground.Deactivate(true);
        }

        // Turn on the new background
        activeBackground = destinationBackground;
        activeBackground.Activate(true);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(GameBackgroundController))]
    public class GameBackgroundControllerEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            GameBackgroundController controller = target as GameBackgroundController;

            if (controller == null || controller.Backgrounds == null)
                return;

            int i = 1;
            foreach (GameBackground background in controller.Backgrounds) {
                if (GUILayout.Button("Transition to BG " + i))
                    controller.TransitionToBackground(background);
                i++;
            }
        }
    }
#endif
}
