using UnityEngine;

/// <summary>
/// Controls a collection of particle systems which together represent a single "bullet" particle system.
/// Includes the fire effect (what the bullet looks like immediately when it is fired) and the visible bullet (what it
/// looks like the rest of the time).
///
/// Managing the particle systems in this way allows us to sync various components between them (seed, emission bursts, size, shape, etc)
/// so that we minimize the amount of work we need to do for each particle system beyond the base configuration. 
/// </summary>
public class BulletParticleSystem : MonoBehaviour {
    public ParticleSystem FireEffectOutline;
    public ParticleSystem FireEffectMiddle;
    public ParticleSystem VisibleOutline;
    public ParticleSystem VisibleMiddle;

    private uint particleSeed;

    private void SyncParticleSystems() {
        SyncParticleSeeds();

        //FireEffectOutline.emission.
    }

    private void SyncParticleSeeds() {
        particleSeed = (uint)Random.Range(0, int.MaxValue);

        FireEffectOutline.randomSeed = particleSeed;
        FireEffectMiddle.randomSeed = particleSeed;
        VisibleOutline.randomSeed = particleSeed;
        VisibleMiddle.randomSeed = particleSeed;
    }

    // Start is called before the first frame update
    void Awake() {
        SyncParticleSystems();
    }
}
