using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a collection of background sprites and textures and stuff that can move around in a coordinated manner
/// </summary>
public class GameBackground : MonoBehaviour {
    private const string BACKGROUND_START_ANIMATION_NAME = "background_start";
    
    // List of every GameObject that is a part of this background
    public List<GameObject> BackgroundElements;

    public List<Animator> Animators;
    
    public void Activate() {
        foreach (GameObject element in BackgroundElements) {
            element.SetActive(true);
        }

        foreach (Animator animator in Animators) {
            animator.Play(BACKGROUND_START_ANIMATION_NAME);
        }
    }

    public void Deactivate() {
        foreach (Animator animator in Animators) {
            animator.StopPlayback();
        }

        foreach (GameObject element in BackgroundElements) {
            element.SetActive(false);
        }
    }
}
