using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scrolls a background across its y axis. Makes a clone of <see cref="BackgroundTile"/> to give the background
/// the appearance of having an infinite scroll. Really, we are just resetting the position every so often. 
/// </summary>
[RequireComponent(typeof(VerticalLayoutGroup))]
public class VerticalBackgroundScroller : MonoBehaviour {
    public float ScrollSpeed = 1f;
    public bool ScrollBottomToTop;
    public GameObject BackgroundTile;
    
    // The height of a single instance of the background tile - after traveling this distance, the 
    // background resets
    private float backgroundHeight;
    private Vector3 startPosition;

    private void Start() {
        // Make a clone of the tile object which gets picked up by the layout group - creates the infinite scroll effect
        Instantiate(BackgroundTile, transform);
        backgroundHeight = BackgroundTile.GetComponent<RectTransform>().rect.height;
        if (ScrollBottomToTop) {
            // We need to move the whole thing up so that the cloned tile appears at the bottom
            transform.position += new Vector3(0, backgroundHeight, 0);
        }
        startPosition = transform.position;
    }

    private void Update() {
        float newPosition = Mathf.Repeat(Time.time * ScrollSpeed, backgroundHeight) * (ScrollBottomToTop ? -1 : 1);
        transform.position = startPosition + Vector3.up * newPosition;
    }
}