using UnityEngine;

public class BoidSupervisorBehaviour : MonoBehaviour
{
    public GameObject boidPrefab;      // for the boid prefab created
    public int boidCount = 50;         // pass the number of boids to participte in the Bird Flocking simulation
    public Vector3 spawnArea = new Vector3(30, 30, 5); // the area to randomly initialize boids
    public Vector3 bounds = new Vector3(30, 30, 5);    // the boundaries that boids are not allowed to pass
    public float disturbRadius = 7f;  // boids within this range will be disturbed from left click
    public float disturbStrength = 10f;

    private GameObject[] boids;       // Array to hold spawned boids

    private const float yOffset = 1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnBoids();  // generate all the requested boids in random position
    }

    // Update is called once per frame
    void Update()
    {
        EnforceBounds();  // check if boids crossed the boundaries (if yes wrap around the space)

        DetectUserForce();  // when mouse left button clicked apply a repulsion force
    }

    private void DetectUserForce()
    {
        // Check for left mouse click to 
        if (Input.GetMouseButtonDown(0))  // if mouse left button clicked return true
        {
            // Get the mouse click position in world space
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 clickPosition = hit.point;
                // Apply repulsion to boids near the click position
                foreach (GameObject boid in boids)
                {
                    Vector3 currentPos = boid.transform.position;
                    currentPos[1] = 0;  // don't take into account the y axis distance;
                    if (boid != null && Vector3.Distance(currentPos, clickPosition) < this.disturbRadius)
                    {
                        boid.GetComponent<BoidBehaviour>().ApplyRepulsionForce(currentPos, clickPosition, disturbStrength, this.disturbRadius);
                    }
                }
            }
        }
    }

    // Spawn boids in the defined area
    private void SpawnBoids()
    {
        boids = new GameObject[boidCount];  // allocate memory for the boids requested
        for (int i = 0; i < boidCount; i++)
        {
            // Generate random positions within the spawn area
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnArea.x / 2, spawnArea.x / 2),
                Random.Range(yOffset, spawnArea.y + yOffset),  // for better visual experience
                Random.Range(-spawnArea.z / 2, spawnArea.z / 2)
            );

            // Spawn the boid and store it in the array
            GameObject boid = Instantiate(boidPrefab, randomPosition, Quaternion.identity);  // instantiate with identity rotation
            boid.name = boidPrefab.name + i;
            boids[i] = boid;
        }
    }

    // Enforce boundary conditions for all boids
    private void EnforceBounds()
    {
        foreach (GameObject boid in boids)
        {
            if (boid == null) continue; // Skip destroyed boids
            Vector3 position = boid.transform.position;

            // Wrap or reflect boids if they exceed bounds
            if (position.x > bounds.x / 2) position.x = -bounds.x / 2;
            else if (position.x < -bounds.x / 2) position.x = bounds.x / 2;

            if (position.y > spawnArea.y + yOffset) position.y = spawnArea.y + yOffset;
            else if (position.y < yOffset) position.y = yOffset;

            if (position.z > bounds.z / 2) position.z = -bounds.z / 2;
            else if (position.z < -bounds.z / 2) position.z = bounds.z / 2;

            // Update the boid's position
            boid.transform.position = position;
        }
    }
}
