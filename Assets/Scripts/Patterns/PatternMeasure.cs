using System;
using UnityEngine;

/// <summary>
/// Single measure for a pattern
/// TODO: Force list size to 32nd notes. Also, like, make a better interface to configure stuff like this.
/// </summary>
[CreateAssetMenu(fileName = "PatternMeasure", menuName = "Resources/Patterns/PatternMeasure", order = 3)]
public class PatternMeasure : ScriptableObject {
    private const int SIZE = 32;

    // List of 32nd notes. It's hard to transcribe sheet music to scriptable objects, okay?
    // 0 indicates for nothing to happen, 1 indicates for a shot to be spawned and do its default thing, 2 is for the previous shot's second thing, 3 for it's third, etc
    // TODO: Using enums or order pairs (index, enum) would be great, but I can't really do that in the inspector and I'd probably need to make subclasses of an object instead of 
    // an enum since PatternController would need the parent class and Shot objects would need to shot-specific sub classes. Can't have objects in the inspector either...
    // TODO: Hey this looks kinda neat https://stackoverflow.com/questions/60864308/how-to-make-an-enum-like-unity-inspector-drop-down-menu-from-a-string-array-with
    // TODO: Functionality for updating a shot that isn't necessarily the most recently fired one
    [Header("32nd note triggers")]
    public int[] Notes = new int[SIZE];

    public Shot Shot;

    private void OnValidate() {
        if (Notes.Length != SIZE) {
            Debug.LogWarning("Don't change the size of 'Notes'!");
            Array.Resize(ref Notes, SIZE);
        }
    }
}
