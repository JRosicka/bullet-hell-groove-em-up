using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An <see cref="IBackgroundElement"/> with a particle system
/// </summary>
[RequireComponent(typeof(Image))]
public class ImageBackgroundElement : MonoBehaviour, IBackgroundElement {
    private Image image;

    private void Awake() {
        image = GetComponent<Image>();
    }

    public void FadeOut(float fadeOutTime) {
        image.CrossFadeAlpha(0, fadeOutTime, false);
    }

    public void FadeIn(float fadeInTime) {
        image.CrossFadeAlpha(1, fadeInTime, false);
    }
}