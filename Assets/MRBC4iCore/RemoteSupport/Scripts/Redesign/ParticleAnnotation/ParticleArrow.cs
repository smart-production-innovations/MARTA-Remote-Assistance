using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ParticleArrow : ParticleAnnotation
{
    public override void StopEmission(float waitTime)
    {
        if (IsEmitting())
        {
            base.StopEmission(waitTime);

            SetEndPoint();
        }
    }

    public void SetEndPoint()
    {
        adjustRemainingLifetime( remainingLifetimeAbsolute: MousePositionCalculation.MaxDisplayTime );

        var colorOverLifetime = ParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
    }

    private ParticleSystem.Particle arrowTipParticle;
    public ParticleSystem.Particle ArrowTipParticle
    {
        get
        {
            if (arrowTipParticle.startSize == 0)
            {
                arrowTipParticle = new ParticleSystem.Particle();
                arrowTipParticle.startLifetime = 1000;
                arrowTipParticle.remainingLifetime = 1000;
                arrowTipParticle.startColor = ParticleSystem.main.startColor.color;
                arrowTipParticle.startSize = ParticleSystem.main.startSize.constant;
                arrowTipParticle.position = transform.position;
            }
            return arrowTipParticle;
        }
    }


    private ParticleSystem.Particle arrowShaftParticle;
    public ParticleSystem.Particle ArrowShaftParticle
    {
        get
        {
            if (arrowShaftParticle.startSize == 0)
            {
                arrowShaftParticle = new ParticleSystem.Particle();
                arrowShaftParticle.startLifetime = ParticleSystem.main.startLifetime.constant;
                arrowShaftParticle.remainingLifetime = ParticleSystem.main.startLifetime.constant;
                arrowShaftParticle.startColor = ParticleSystem.main.startColor.color;
                arrowShaftParticle.startSize = ParticleSystem.main.startSize.constant;
            }
            arrowShaftParticle.position = transform.position;
            return arrowShaftParticle;
        }
    }

    public float ArrowLength
    {
        get
        {
            return Vector3.Distance(transform.position, startPoint);
        }
    }

    private void Update()
    {
        if (IsEmitting())
        {
            var trails = ParticleSystem.trails;
            var arrowLength = ArrowLength * 0.5f;
            trails.widthOverTrailMultiplier = arrowLength;
        }
    }

    private Vector3 startPoint;
    private void Start()
    {
        startPoint = transform.position;

        var startParticle = new ParticleSystem.Particle[] { ArrowTipParticle };
        ParticleSystem.SetParticles(startParticle);
    }
}
