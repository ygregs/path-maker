// Create a Particle System
// Set a 5 second start delay for the system, and a 2 second lifetime for each particle
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSpeed : MonoBehaviour
{
    private ParticleSystem ps;
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
        var psdur = ps.main;
        psdur.duration = 1.0f;

        ps.Play();
    }
}