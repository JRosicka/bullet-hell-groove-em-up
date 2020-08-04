using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Controls a collection of particle systems which together represent a single "bullet" particle system.
///
/// Can include any number of nested BulletParticleSystems and ParticleSystems which it retrieves from its child game objects. 
/// Includes the fire effect (what the bullet looks like immediately when it is fired) and the visible bullet (what it
/// looks like the rest of the time).
///
/// Managing the particle systems in this way allows us to sync their values from templates
/// so that we minimize the amount of work we need to do for each particle system beyond the initial configuration.
///
/// The ParticleSystem attached to this GameObject will be destroyed at runtime
/// </summary>
[ExecuteInEditMode]
public class BulletParticleSystem : MonoBehaviour {
    private uint particleSeed;
    private List<BulletParticleSystem> subBSystems;
    private List<ParticleSystem> subPSystems;

    public void SetParticleSeed(uint newParticleSeed) {
        particleSeed = newParticleSeed;
    }

    void Awake() {
        if (IsRootBulletParticleSystem())
            SyncParticleSeeds(true);
        
        if (Application.isPlaying)
            Destroy(GetComponent<ParticleSystem>());

        subBSystems = GetComponentsInChildren<BulletParticleSystem>(false).ToList();
        subPSystems = GetComponentsInChildren<ParticleSystem>(false).ToList();
    }
    
    #if UNITY_EDITOR
    public void SyncParticleSystems() {
        ParticleSystem templatePSystem = GetComponent<ParticleSystem>();
        Assert.IsNotNull(templatePSystem, "Need to attach a ParticleSystem component!");
        UnityEditorInternal.ComponentUtility.CopyComponent(templatePSystem);
        foreach (ParticleSystem pSystem in GetImmediateChildrenParticleSystems()) {
            UnityEditorInternal.ComponentUtility.PasteComponentValues(pSystem);
            pSystem.randomSeed = particleSeed;
        }
        
        foreach (BulletParticleSystem bSystem in GetImmediateChildrenBulletParticleSystems()) {
            bSystem.SyncParticleSystems();
            bSystem.SetParticleSeed(particleSeed);
        }
    }
    #endif

    // Trigger all of the particle systems to start
    public void Shoot() {
        foreach (ParticleSystem pSystem in subPSystems) {
            if (pSystem)
                pSystem.Play();
        }
    }

    public void StopAll() {
        foreach (ParticleSystem pSystem in subPSystems) {
            if (pSystem)
                pSystem.Stop();
        }
    }

    public List<ParticleSystem> GetActiveParticleSystems() {
        return subPSystems;
    }

    private List<BulletParticleSystem> GetImmediateChildrenBulletParticleSystems() {
        return GetComponentsInChildren<BulletParticleSystem>(true).Where(e => (e.transform.parent == transform))
            .ToList();
    }
    
    private List<ParticleSystem> GetImmediateChildrenParticleSystems() {
        return GetComponentsInChildren<ParticleSystem>(true).Where(e => (e.transform.parent == transform))
            .ToList();
    }

    private bool IsRootBulletParticleSystem() {
        return GetComponentsInParent<BulletParticleSystem>(true).Length <= 1;
    }
    
    private void SyncParticleSeeds(bool generateNew) {
        if (generateNew)
            particleSeed = (uint) Random.Range(0, int.MaxValue);
        
        foreach (ParticleSystem pSystem in GetComponentsInChildren<ParticleSystem>(true)) {
            pSystem.randomSeed = particleSeed;
        }

        foreach (BulletParticleSystem bSystem in GetComponentsInChildren<BulletParticleSystem>(true)) {
            bSystem.SetParticleSeed(particleSeed);
        }
    }
    
#if UNITY_EDITOR
    [CustomEditor(typeof(BulletParticleSystem))]
    public class PatternMeasureEditor : Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            BulletParticleSystem bSystem = target as BulletParticleSystem;
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Sync Particle Systems")) {
                if (bSystem == null) return;
                ParticleSystem pSystem = bSystem.GetComponent<ParticleSystem>();
                Assert.IsNotNull(pSystem, "Need to attach a ParticleSystem component!");
                pSystem.Stop();
                bSystem.StopAll();
                    
                bSystem.SyncParticleSystems();
                bSystem.SyncParticleSeeds(true);
            }
        }
    }
#endif
}
