using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents a collection of background sprites and textures and stuff that can move around in a coordinated manner
/// </summary>
public class GameBackground : MonoBehaviour {
    // List of every GameObject that is a part of this background
    private List<BackgroundScroller> BackgroundElements;
    public bool ActiveAtStart;

    public void Initialize() {
        BackgroundElements = GetComponentsInChildren<BackgroundScroller>().ToList();
        BackgroundElements.ForEach(bg => bg.Initialize());
    }
    
    public void Activate(bool fadeIn) {
        foreach (BackgroundScroller scroller in BackgroundElements) {
            if (!scroller.isActiveAndEnabled) {
                continue;
            }
            if (fadeIn) {
                // scroller.AllOff();
                scroller.FadeIn();
            } else {
                scroller.AllOn();
            }
        }
    }

    public void Deactivate(bool fadeOut) {
        foreach (BackgroundScroller scroller in BackgroundElements) {
            if (!scroller.isActiveAndEnabled) {
                continue;
            }
            if (fadeOut) {
                scroller.FadeOut();
            } else {
                scroller.AllOff();
            }
        }
    }
}
