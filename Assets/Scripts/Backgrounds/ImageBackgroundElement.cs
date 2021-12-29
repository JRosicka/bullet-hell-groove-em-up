using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// An <see cref="IBackgroundElement"/> with an image or set of images
/// </summary>
public class ImageBackgroundElement : MonoBehaviour, IBackgroundElement {
    private List<Image> images;

    private void Awake() {
        images = GetComponentsInChildren<Image>().ToList();
    }

    public void FadeOut(float fadeOutTime) {
        images.ForEach(image => {
            image.CrossFadeAlpha(0, fadeOutTime, false);
        });
    }

    public void FadeIn(float fadeInTime) {
        images.ForEach(image => {
            image.CrossFadeAlpha(1, fadeInTime, false);
        });
    }
}