using System.Collections;
using System.Collections.Generic;
using Rumia;
using UnityEngine;

public class ExileChantPattern : Pattern
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    #region RumiaActions

        // ReSharper disable once UnusedMember.Global
        [RumiaAction]
        public void Dummy() {
            Debug.Log("Dummy pattern: Chant");
        }

        // ReSharper disable once UnusedMember.Global
        [RumiaAction]
        public void Move() {
            
        }

    #endregion
}
