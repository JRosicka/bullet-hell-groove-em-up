using System;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

/// <summary>
/// Scrolls a background across its y axis. Really, we are just resetting the position every so often. 
/// </summary>
[RequireComponent(typeof(VerticalLayoutGroup))]
public class VerticalBackgroundScroller : BackgroundScroller {
    public bool ScrollBottomToTop;
    
    // The height of a single instance of the background tile - after traveling this distance, the 
    // background resets
    private float backgroundHeight;
    private Vector3 startPosition;
    private float timeOffset;

    public override void Initialize() {
        // Make a clone of the tile object which gets picked up by the layout group - creates the infinite scroll effect
        Instantiate(BackgroundTile, transform);
        backgroundHeight = BackgroundTile.GetComponent<RectTransform>().rect.height;
        if (!ScrollBottomToTop) {
            float angle = Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI / 2;

            // We need to move the whole thing up so that the cloned tile appears at the bottom
            transform.position += new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * backgroundHeight;
        }
        startPosition = transform.position;
        backgroundElements = transform.GetComponentsInChildren<IBackgroundElement>().ToList();
    }
    
    private void Update() {
        float newPosition = Mathf.Repeat((Time.time + timeOffset) * ScrollSpeed, backgroundHeight) 
                            * (ScrollBottomToTop ? 1 : -1);
        float angle = Mathf.Deg2Rad * transform.localEulerAngles.z + Mathf.PI / 2;
        Vector3 vector = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
        transform.position = startPosition + vector * newPosition;
    }
    
    public override void ResetBackground() {
        transform.position = startPosition;
        timeOffset = -Time.time;
    }
}