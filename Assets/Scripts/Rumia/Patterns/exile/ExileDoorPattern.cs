using System.Collections;
using System.Collections.Generic;
using Rumia;
using UnityEngine;

public class ExileDoorPattern : Pattern {
    public Animator DoorAnimator;
    
    #region RumiaActions

    [RumiaAction]
    public void ShowDoor() {
        transform.SetAsLastSibling();
        DoorAnimator.Play("Door open");
    }

    [RumiaAction]
    public void HideDoor() {
        DoorAnimator.Play("Door close");
    }

    #endregion

}