using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles displaying and updating stage information presented on the overlay, like lives bombs and stage name. 
/// </summary>
public class StageUIView : MonoBehaviour {
    public List<UIContainerView> Lives;
    public List<UIContainerView> Bombs;

    private void Awake() {
        EventManager.LifeCountChangeEvent.AddListener(OnLifeCountChanged);
        EventManager.BombCountChangeEvent.AddListener(OnBombCountChanged);
    }

    private void OnLifeCountChanged(int newCount) {
        UpdateContainerSet(Lives, newCount);
    }
    
    private void OnBombCountChanged(int newCount) {
        UpdateContainerSet(Bombs, newCount);
    }

    private void UpdateContainerSet(List<UIContainerView> containers, int newCount) {
        newCount = Mathf.Clamp(newCount, 0, containers.Count);
        for (int i = 0; i < newCount; i++) {
            containers[i].ToggleContainer(true);
        }

        for (int i = newCount; i < containers.Count; i++) {
            containers[i].ToggleContainer(false);
        }
    }
}
