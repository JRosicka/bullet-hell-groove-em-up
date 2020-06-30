using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class SyncParticleSeeds : MonoBehaviour
{

    public ParticleSystem[] setSeedParticles;
    private int particleSeed;

    void Start()
    {
        particleSeed = Random.Range(0, int.MaxValue);

        if (setSeedParticles != null && setSeedParticles.Length > 0)
        {
            for (int i = 0; i < setSeedParticles.Length; i++)
            {
                setSeedParticles[i].randomSeed = (uint)particleSeed;
            }
        }
    }
}