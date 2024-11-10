using System.Collections.Generic;
using UnityEngine;

public class SupervisorBehaviour1 : MonoBehaviour
{
    // public variables for wind force
    public float kWind = 0.8f;
    public Vector3 vWind = new(5.5f, 0f, 0f);

    public float epsilon = 0.7f; // coefficient of restitution

    public Bounds bounds = new Bounds(new Vector3(5f, 5f, 1f), new Vector3(4f, 4f, 0.2f));  // center (5,5,5) and range  (10,10,10)

    public int maxParticlesNum = 300;

    public int generateCounter = 20;  // number of updates passed to create a new

    public GameObject particlePrefab = null;

    private List<GameObject> particles = new List<GameObject>();

    private int localCounter;

    private int nameCounter = 0;

    void generateParticle()
    {
        if (localCounter > 0)
        {
            localCounter--;
            return;
        }
        if (particles.Count >= maxParticlesNum) return;
        if (particlePrefab != null)
        {
            GameObject particle = Instantiate(particlePrefab);
            particle.name = particlePrefab.name + nameCounter;
            nameCounter++;
            particles.Add(particle);
            localCounter = generateCounter;
        }
        else
        {
            Debug.LogError("Please assign a particle Prefab.");
        }

    }

    void avoidOverflow()
    {
        if (particles.Count >= maxParticlesNum) {
            for (int i = 0; i < maxParticlesNum / 5; ++i)
            {
                Destroy(particles[i]);
                particles.RemoveAt(i);
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        localCounter = generateCounter;
        generateParticle();
    }

    // Update is called once per frame
    void Update()
    {
        generateParticle();
        avoidOverflow();
    }
}
