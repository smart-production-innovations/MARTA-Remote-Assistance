using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// type of AR features
/// </summary>
public enum ARFeatureType
{
    None,
    Points,
    Planes
}

/// <summary>
/// defines how AR features, such as found spatial planes and point clouds, are displayed
/// </summary>
public class ARFeatureVisualization : MonoBehaviour
{
    public Color defaultFeatureColor;

    private void OnEnable()
    {
        setActualDefault();
    }

    /// <summary>
    /// defines if the assigned feater type is displayed by default
    /// </summary>
    private void setActualDefault()
    {
        if (FeatureType == ARFeatureType.Planes)
            DisplayFeature = StatusProperties.Values.ShowARPlanes;
        else if (FeatureType == ARFeatureType.Points)
            DisplayFeature = StatusProperties.Values.ShowARFeaturePoints;
    }

    /// <summary>
    /// defines if the assigned feater type is displayed
    /// </summary>
    public bool DisplayFeature
    {
        get
        {
            var mesh = GetComponent<MeshRenderer>();
            if (mesh)
            {
                return (mesh.material.color.a == defaultFeatureColor.a);
            }
            else
            {
                var particle = GetComponent<ParticleSystem>();
                if (particle)
                {
                    var main = particle.main;
                    return (main.startColor.color.a == defaultFeatureColor.a);
                }
            }
            return true;
        }
        set
        {
            var newColor = new Color(defaultFeatureColor.r, defaultFeatureColor.g, defaultFeatureColor.b, (value ? defaultFeatureColor.a : 0));
            var mesh = GetComponent<MeshRenderer>();
            if (mesh)
            {
                mesh.material.color = newColor;
            }
            else
            {
                var particle = GetComponent<ParticleSystem>();
                if (particle)
                {
                    var main = particle.main;
                    main.startColor = newColor;

                    ParticleSystem.Particle[] m_Particles = new ParticleSystem.Particle[particle.main.maxParticles];
                    int numParticlesAlive = particle.GetParticles(m_Particles);
                    for (int i = 0; i < numParticlesAlive; i++)
                    {
                        m_Particles[i].color = newColor;
                    }
                }
            }
        }
    }

    /// <summary>
    /// defines the assigned feater type
    /// </summary>
    public ARFeatureType FeatureType
    {
        get
        {
            if (GetComponent<MeshRenderer>())
                return ARFeatureType.Planes;
            if (GetComponent<ParticleSystem>())
                return ARFeatureType.Points;
            return ARFeatureType.None;
        }
    }
}
