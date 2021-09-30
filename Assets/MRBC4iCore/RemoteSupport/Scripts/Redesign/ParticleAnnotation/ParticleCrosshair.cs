using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCrosshair : ParticleAnnotation
{
    private void Start()
    {
        var particleSize = ParticleSystem.main.startSize.constant;

        var startParticle = new ParticleSystem.Particle[4];
        for (int i = 0; i < startParticle.Length; i++)
        {
            startParticle[i].startLifetime = ParticleSystem.main.startLifetime.constant;
            startParticle[i].remainingLifetime = ParticleSystem.main.startLifetime.constant / startParticle.Length * i;
            startParticle[i].startColor = ParticleSystem.main.startColor.color;
            startParticle[i].startSize = particleSize * initSize;
            startParticle[i].position = Vector3.zero;
        }
        ParticleSystem.SetParticles(startParticle);
    }

    public override void StopEmission(float waitTime)
    {
        if (IsEmitting())
        {
            base.StopEmission(waitTime);

            var sizeOverLifetime = ParticleSystem.sizeOverLifetime;
            sizeOverLifetime.enabled = false;

            adjustParticle();
            adjustRemainingLifetime(remainingLifetimeRelative: MousePositionCalculation.MaxDisplayTime);

            var main = ParticleSystem.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var colorOverLifetime = ParticleSystem.colorOverLifetime;
            var gradient = colorOverLifetime.color.gradient;
            gradient.alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(0, 1), new GradientAlphaKey(1, 0) };
        }
    }

    protected virtual void adjustParticle()
    {
        ParticleSystem.Particle[] activeParticles = GetParticles();

        for (int i = 0; i < activeParticles.Length; i++)
        {
            activeParticles[i].startColor = activeParticles[i].GetCurrentColor(ParticleSystem);
            activeParticles[i].position = transform.position;
        }

        ParticleSystem.SetParticles(activeParticles);
    }

    public override void setSize(float size)
    {
        base.setSize(size); 
    }
}
