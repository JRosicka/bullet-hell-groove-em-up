using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// A controllable piece of a background that moves around textures and images and stuff. Makes clones of
/// <see cref="BackgroundTile"/> to give the background the appearance of having an infinite scroll/rotate.
/// </summary>
public abstract class BackgroundScroller : MonoBehaviour {
    public GameObject BackgroundTile;
    public float FadeTime = 1.5f;
    public float ScrollSpeed = 1f;
    protected List<IBackgroundElement> backgroundElements;
    
    public void AllOn() {
        backgroundElements.ForEach(element => {
            element.FadeIn(0f);
        });
    }
    
    public void AllOff() {
        backgroundElements.ForEach(element => {
            element.FadeOut(0f);
        });
    }
    
    [Button]
    public void FadeIn() {
        backgroundElements.ForEach(element => {
            element.FadeIn(FadeTime);
        });
    }
    
    [Button]
    public void FadeOut() {
        backgroundElements.ForEach(element => {
            element.FadeOut(FadeTime);
        });
    }

    public abstract void Initialize();
    
    [Button]
    public abstract void ResetBackground();
}