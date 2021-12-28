using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Scrolls a background by rotating it about its z axis. 
/// </summary>
[RequireComponent(typeof(RadialLayout))]
public class RadialBackgroundScroller : BackgroundScroller {
    public bool ScrollCounterClockwise;
    [Range(1, 20)]
    public int NumberOfTiles = 1;
    private Quaternion startRotation;

    [SerializeField] [HideInInspector]
    private int serializedNumberOfTiles;
    
    private void Update() {
        transform.Rotate(0, 0, Time.deltaTime * ScrollSpeed * (ScrollCounterClockwise ? -1 : 1));
    }

#if UNITY_EDITOR
    private void OnValidate() {
        if (NumberOfTiles != serializedNumberOfTiles) {
            serializedNumberOfTiles = NumberOfTiles;
            StartCoroutine(SetTilesInABit());
        }
    }

    private IEnumerator SetTilesInABit() {
        yield return new WaitForSeconds(.05f);
        SetTiles();
    }
#endif
    
    public void SetTiles() {
        List<Transform> transforms = new List<Transform>();
        for (int i = 0; i < transform.childCount; i++) {
            transforms.Add(transform.GetChild(i));
        }
        foreach (Transform trans in transforms) {
            if (gameObject.transform.GetChild(0) == trans) {
                continue;
            }
            DestroyImmediate(trans.gameObject, true);
        }
        
        // Make clones of the tile object which get picked up by the layout group - creates the infinite rotation effect
        for (int i = 1; i < NumberOfTiles; i++) {
            Instantiate(BackgroundTile, transform);
        }
    }

    public override void Initialize() {
        SetTiles();
        startRotation = transform.localRotation;
        backgroundElements = transform.GetComponentsInChildren<IBackgroundElement>().ToList();
    }

    public override void ResetBackground() {
        transform.localRotation = startRotation;
    }
}