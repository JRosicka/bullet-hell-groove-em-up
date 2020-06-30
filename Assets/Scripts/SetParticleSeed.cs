using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SetParticleSeed : MonoBehaviour
{

    public ParticleSystem[] setSeedParticles;
    public int ParticleSeed;
    public bool forceNewSeed;

    void Start()
    {
        if (setSeedParticles != null && setSeedParticles.Length > 0)
        {
            for (int i = 0; i < setSeedParticles.Length; i++)
            {
                setSeedParticles[i].randomSeed = (uint)ParticleSeed;
            }
        }
    }

    void Update()
    {
        if (forceNewSeed)
        {
            ParticleSeed = Random.Range(0, int.MaxValue);

            if (setSeedParticles.Length > 0)
            {
                for (int i = 0; i < setSeedParticles.Length; i++)
                {
                    setSeedParticles[i].randomSeed = (uint)ParticleSeed;
                }
            }

            forceNewSeed = false;
        }
    }
}