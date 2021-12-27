using UnityEngine;

/// <summary>
/// Scrolls a background by rotating it about its z axis. 
/// </summary>
[RequireComponent(typeof(RadialLayout))]
public class RadialBackgroundScroller : MonoBehaviour {
    public float RotateSpeed = 1f;
    public bool ScrollCounterClockwise;
    [Range(1, 20)]
    public int NumberOfTiles = 1;
    public GameObject BackgroundTile;

    private void Start() {
        SetTiles();
    }

    private void Update() {
        transform.Rotate(0, 0, Time.deltaTime * RotateSpeed * (ScrollCounterClockwise ? -1 : 1));
    }
    
    public void SetTiles() {
        foreach (Transform trans in transform) {
            if (gameObject.transform.GetChild(0) == trans) {
                continue;
            }
            Destroy(trans);
        }
        
        // Make clones of the tile object which get picked up by the layout group - creates the infinite rotation effect
        for (int i = 1; i < NumberOfTiles; i++) {
            Instantiate(BackgroundTile, transform);
        }
    }
}