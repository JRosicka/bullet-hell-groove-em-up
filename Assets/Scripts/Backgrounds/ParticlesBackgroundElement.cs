using UnityEngine;

/// <summary>
/// An <see cref="IBackgroundElement"/> with a particle system
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class ParticlesBackgroundElement : MonoBehaviour, IBackgroundElement {
    private ParticleSystem pSystem;

    private float fadeTime;
    private float currentTime;

    private enum FadeState {
        None,
        FadingOut,
        FadingIn
    }
    private FadeState fadeState;
    
    void Awake() {
        pSystem = GetComponent<ParticleSystem>();
    }

    private void Update() {
        switch (fadeState) {
            case FadeState.None:
                break;
            case FadeState.FadingIn:
                currentTime += Time.deltaTime;
                SetParticleAlphas(1 - (fadeTime - currentTime) / fadeTime);
                if (currentTime >= fadeTime) {
                    fadeState = FadeState.None;
                }
                break;
            case FadeState.FadingOut:
                currentTime += Time.deltaTime;
                SetParticleAlphas((fadeTime - currentTime) / fadeTime);
                if (currentTime >= fadeTime) {
                    pSystem.Pause();
                    fadeState = FadeState.None;
                }
                break;
        }
    }

    private void SetParticleAlphas(float newAlpha) {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[pSystem.particleCount];
        pSystem.GetParticles(particles);

        for (int i = 0; i < particles.Length; i++) {
            Color color = particles[i].startColor;
            color.a = Mathf.Clamp(newAlpha, 0f, 1f);
            particles[i].startColor = color;
        }
        pSystem.SetParticles(particles, particles.Length);
    }

    public void FadeOut(float fadeOutTime) {
        fadeTime = fadeOutTime;
        currentTime = 0f;
        fadeState = FadeState.FadingOut;
    }
    
    public void FadeIn(float fadeInTime) {
        fadeTime = fadeInTime;
        currentTime = 0f;
        fadeState = FadeState.FadingIn;
        pSystem.Play();
    }
}