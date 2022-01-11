using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles setting up and updating the progress bar which visualizes how far along in the stage the player is
/// </summary>
public class ProgressBar : MonoBehaviour {
    public GameObject ProgressMarker;
    public RectTransform ClearedProgressBar;
    public GameObject SectionPrefab;
    public GameObject SectionDividerPrefab;
    public Transform ProgressSectionsBucket;
    public Transform SectionDividersBucket;
    
    public float YMin, YMax;
    // The amount of space between the start and end locations (YMin and YMax) for the marker and the edges of the 
    // progress bar. We need to fill this extra space by extending the first and last sections of the bar. 
    public float ExtraSpaceBetweenMarkerAndEdges;
    private float startTime;
    private float stageTimeLength;
    private float elapsedTime;
    
    private float TotalYLength => YMax - YMin;
    private float ProgressBarOffset => ((RectTransform) transform).rect.height / 2;

    void Start() {
        SetUpProgressBar(new List<float> {
            6f, 10f, 12.2f, 15f, 19.5f, 23f, 25.1f
        });
    }

    /// <summary>
    /// Creates and places progress bar sections based on the provided transition times
    /// </summary>
    /// <param name="transitionTimes">Timestamp of each section of the song where a transition occurs,
    /// including the start and end times.</param>
    public void SetUpProgressBar(List<float> transitionTimes) {
        ProgressMarker.transform.localPosition = new Vector2(0, YMin - ProgressBarOffset);

        startTime = transitionTimes.First();
        stageTimeLength = transitionTimes.Last() - startTime;
        // Instantiate, translate, and stretch each section piece
        for (int i = 1; i < transitionTimes.Count; i++) {
            float sectionTimeLength = transitionTimes[i] - transitionTimes[i - 1];
            float yLength = sectionTimeLength / stageTimeLength * TotalYLength;
            float yLocation = (transitionTimes[i - 1] - transitionTimes.First()) 
                / stageTimeLength * TotalYLength
                + ExtraSpaceBetweenMarkerAndEdges;

            // Account for the added space for the first and last pieces
            if (i == 1) {
                yLength += ExtraSpaceBetweenMarkerAndEdges;
                yLocation -= ExtraSpaceBetweenMarkerAndEdges;
            } else if (i == transitionTimes.Count - 1) {
                yLength += ExtraSpaceBetweenMarkerAndEdges;
            }
            
            GameObject newSection = Instantiate(SectionPrefab, ProgressSectionsBucket);
            RectTransform rt = (RectTransform) newSection.transform;
            rt.sizeDelta = new Vector2(rt.rect.width, yLength);
            rt.localPosition = new Vector2(0, yLocation);

            if (i > 1) {
                GameObject newSectionDivider = Instantiate(SectionDividerPrefab, SectionDividersBucket);
                newSectionDivider.transform.localPosition = new Vector2(0, yLocation);
            }

            // Set the piece's color
            newSection.GetComponent<Image>().color = Color.Lerp(Color.black, Color.blue, (i - 1f) / (transitionTimes.Count - 1f));
        }
    }

    void Update() {
        elapsedTime += Time.deltaTime;
        if (elapsedTime < startTime || elapsedTime - startTime > stageTimeLength) 
            return;

        // Update the progress marker's location and darken the elapsed part of the progress bar
        float stageCompletionProgress = (elapsedTime - startTime) / stageTimeLength;
        float newPosition = Mathf.Lerp(YMin, YMax, stageCompletionProgress) - ProgressBarOffset;
        ProgressMarker.transform.localPosition = new Vector2(0, newPosition);
        ClearedProgressBar.sizeDelta = new Vector2(ClearedProgressBar.rect.width, 
                newPosition 
                + ExtraSpaceBetweenMarkerAndEdges / 2
                + ProgressBarOffset);
    }
}
