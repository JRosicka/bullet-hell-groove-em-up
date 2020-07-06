using UnityEngine;

/// <summary>
/// Automatically syncs a set of particle systems to the same seed
/// </summary>
[ExecuteInEditMode]
public class SyncParticleSeeds : MonoBehaviour {

    public ParticleSystem[] setSeedParticles;
    private int particleSeed;

    void Start() {
        particleSeed = Random.Range(0, int.MaxValue);

        if (setSeedParticles != null && setSeedParticles.Length > 0) {
            foreach (ParticleSystem system in setSeedParticles) {
                system.randomSeed = (uint)particleSeed;
            }
        }
    }
}