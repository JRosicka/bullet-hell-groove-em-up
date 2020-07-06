using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Create a new Shot instance in the scene
/// </summary>
public class FireShotNoteAction : NoteAction
{
    private Transform spawner;
    private Shot shotPrefab;

    public FireShotNoteAction(int index, float triggerTime, List<Shot> shotInstances, Shot shotPrefab, Transform spawner) : base(index, triggerTime, shotInstances) {
        this.spawner = spawner;
        this.shotPrefab = shotPrefab;
    }
    
    /// <summary>
    /// Shoot your shot.Shot, my dude
    /// </summary>
    public override void PerformAction() {
        Shot shotInstance = Object.Instantiate(shotPrefab, spawner);
        ShotInstances.Add(shotInstance);
    }
}
