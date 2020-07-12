using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create a new Shot instance in the scene
/// </summary>
public class FireShotNoteAction : NoteAction {
    private Transform spawner;
    private Shot shotPrefab;
    private readonly List<ConfigurationEvent> shotInstances;

    public FireShotNoteAction(int index, float triggerTime, List<ConfigurationEvent> shotInstances, Shot shotPrefab, Transform spawner) : base(index, triggerTime) {
        this.spawner = spawner;
        this.shotPrefab = shotPrefab;
        this.shotInstances = shotInstances;
    }
    
    /// <summary>
    /// Shoot your shot.Shot, my dude
    /// </summary>
    public override void PerformAction() {
        Shot shotInstance = Object.Instantiate(shotPrefab, spawner.position, spawner.rotation, PlayerController.Instance.ShotBucket);
        shotInstances.Add(shotInstance);
    }
}
