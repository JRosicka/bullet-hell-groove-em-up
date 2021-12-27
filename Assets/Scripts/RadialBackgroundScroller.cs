using System.Collections;
using System.Collections.Generic;
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

    [SerializeField] [HideInInspector]
    private int serializedNumberOfTiles;
    
    private void Start() {
        SetTiles();
    }

    private void Update() {
        transform.Rotate(0, 0, Time.deltaTime * RotateSpeed * (ScrollCounterClockwise ? -1 : 1));
    }

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
}