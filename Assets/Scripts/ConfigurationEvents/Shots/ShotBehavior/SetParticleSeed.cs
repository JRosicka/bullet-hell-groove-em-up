using UnityEngine;

/// <summary>
/// Set a specific particle seed to a set of particle systems
/// </summary>
[ExecuteInEditMode]
public class SetParticleSeed : MonoBehaviour {

    public ParticleSystem[] setSeedParticles;
    public int ParticleSeed;
    public bool forceNewSeed;

    void Start() {
        if (setSeedParticles != null && setSeedParticles.Length > 0) {
            foreach (ParticleSystem system in setSeedParticles) {
                system.randomSeed = (uint)ParticleSeed;
            }
        }
    }

    void Update() {
        if (forceNewSeed) {
            ParticleSeed = Random.Range(0, int.MaxValue);

            if (setSeedParticles.Length > 0) {
                foreach (ParticleSystem system in setSeedParticles) {
                    system.randomSeed = (uint)ParticleSeed;
                }
            }

            forceNewSeed = false;
        }
    }
}