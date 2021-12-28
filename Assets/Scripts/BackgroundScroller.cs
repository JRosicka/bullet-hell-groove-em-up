using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A controllable piece of a background that moves around textures and images and stuff. Makes clones of
/// <see cref="BackgroundTile"/> to give the background the appearance of having an infinite scroll/rotate.
/// </summary>
public abstract class BackgroundScroller : MonoBehaviour {
    public GameObject BackgroundTile;
    public float FadeTime = 1.5f;
    public float ScrollSpeed = 1f;
    protected List<Image> allBackgroundImages;
    
    public void AllOn() {
        allBackgroundImages.ForEach(image => {
            image.CrossFadeAlpha(1, 0, false);
            // Color color = image.color;
            // image.color = new Color(color.r, color.g, color.b, 1f);
        });
    }
    
    public void AllOff() {
        allBackgroundImages.ForEach(image => {
            image.CrossFadeAlpha(0, 0f, false);
            // Color color = image.color;
            // image.color = new Color(color.r, color.g, color.b, 0f);
        });
    }
    
    [Button]
    public void FadeIn() {
        allBackgroundImages.ForEach(image => image.CrossFadeAlpha(1, FadeTime, false));
    }
    
    [Button]
    public void FadeOut() {
        allBackgroundImages.ForEach(image => image.CrossFadeAlpha(0, FadeTime, false));
    }

    public abstract void Initialize();
    
    [Button]
    public abstract void ResetBackground();
}