using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleLine : ParticleAnnotation
{
    private void Awake()
    {
        var main = ParticleSystem.main;
        main.startLifetime = MousePositionCalculation.MaxDisplayTime * 4;
    }
}
