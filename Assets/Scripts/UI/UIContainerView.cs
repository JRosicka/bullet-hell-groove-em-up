using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a container for a UI element (life or bomb)
/// </summary>
public class UIContainerView : MonoBehaviour {
    public Image ToggleableImage;
    public Color ActiveColor;
    public Color InactiveColor;
    
    public void ToggleContainer(bool active) {
        ToggleableImage.color = active ? ActiveColor : InactiveColor;
    }
}