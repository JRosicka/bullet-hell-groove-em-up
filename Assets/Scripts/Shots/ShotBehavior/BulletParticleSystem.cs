using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletParticleSystem : MonoBehaviour {
    public ParticleSystem FireEffectOutline;
    public ParticleSystem FireEffectMiddle;
    public ParticleSystem VisibleOutline;
    public ParticleSystem VisibleMiddle;

    private uint particleSeed;

    private void syncParticleSystems() {
        syncParticleSeeds();

        //FireEffectOutline.emission.
    }

    private void syncParticleSeeds() {
        particleSeed = (uint)Random.Range(0, int.MaxValue);

        FireEffectOutline.randomSeed = particleSeed;
        FireEffectMiddle.randomSeed = particleSeed;
        VisibleOutline.randomSeed = particleSeed;
        VisibleMiddle.randomSeed = particleSeed;
    }

    // Start is called before the first frame update
    void Awake() {
        syncParticleSystems();
    }
}
