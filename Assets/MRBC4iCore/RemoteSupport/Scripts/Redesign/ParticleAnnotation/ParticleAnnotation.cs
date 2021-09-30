using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleAnnotation : MonoBehaviour
{
    private ParticleSystem particleSystem;
    public ParticleSystem ParticleSystem
    {
        get
        {
            if (!particleSystem)
                particleSystem = GetComponent<ParticleSystem>();
            return particleSystem;
        }
    }

    public virtual void StopEmission(float waitTime)
    {
        var emission = ParticleSystem.emission;
        emission.rateOverTime = 0;
    }

    public void SetColor(Color color)
    {
        var main = ParticleSystem.main;
        main.startColor = color;
    }
    public bool hasParticles()
    {
        if (IsEmitting())
            return true;
        return (ParticleSystem.particleCount > 0);
    }

    public bool IsEmitting()
    {
        return (ParticleSystem.emission.rateOverTime.constant > 0);
    }

    protected virtual void adjustRemainingLifetime(float remainingLifetimeAbsolute = 0, float remainingLifetimeRelative = 0, float remainingLifetimeIncremental = 0)
    {
        ParticleSystem.Particle[] activeParticles = GetParticles();
        for (int i = 0; i < activeParticles.Length; i++)
        {
            if (remainingLifetimeAbsolute > 0)
            {
                activeParticles[i].startLifetime = remainingLifetimeAbsolute;
                activeParticles[i].remainingLifetime = remainingLifetimeAbsolute;
            }

            if (remainingLifetimeRelative > 0)
            {
                activeParticles[i].startLifetime += remainingLifetimeRelative;
                activeParticles[i].remainingLifetime += remainingLifetimeRelative;
            }

            if (remainingLifetimeIncremental > 0)
            {
                activeParticles[i].startLifetime += remainingLifetimeIncremental * i;
            }
        }

        ParticleSystem.SetParticles(activeParticles);
    }

    protected ParticleSystem.Particle[] GetParticles()
    {
        ParticleSystem.Particle[] activeParticles = new ParticleSystem.Particle[ParticleSystem.main.maxParticles];
        int count = ParticleSystem.GetParticles(activeParticles);

        ParticleSystem.Particle[] usedParticles = new ParticleSystem.Particle[count];
        Array.Copy(activeParticles, 0, usedParticles, 0, count);
        return usedParticles;
    }

    protected float initSize = 0;
    public virtual void setSize(float size)
    {
        var main = ParticleSystem.main;
        var startSize = main.startSize;
        if (initSize <= 0)
            initSize = startSize.constant;

        startSize.constant = size * initSize * 0.5f;
        main.startSize = startSize;
    }
}
