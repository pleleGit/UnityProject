using System.Collections.Generic;
using UnityEngine;

public class SupervisorBehaviour2 : MonoBehaviour
{
    public Vector3 center = new Vector3(5f, 5f, 5f);  // center of boundary sphere

    public float radius = 4f;  // radius of boundary sphere

    public int ParticlesNum = 20;

    public GameObject particlePrefab = null;

    private List<GameObject> particles = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generateParticles(ParticlesNum);
    }

    // Update is called once per frame
    void Update()
    {
    }
    void generateParticles(int particleNum)
    {

        if (particlePrefab != null)
        {
            for (int i = 0; i < particleNum; ++i)
            {
                GameObject particle = Instantiate(particlePrefab);
                particle.name = particlePrefab.name.Substring(0, 10) + i;
                particles.Add(particle);
            }
        }
        else
        {
            Debug.LogError("Please assign a particle Prefab.");
        }

    }

    public List<GameObject> getGameObjects()
    {
        return particles;
    }

}
