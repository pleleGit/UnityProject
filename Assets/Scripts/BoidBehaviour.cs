using UnityEngine;
using System.Collections.Generic;

public class BoidBehaviour : MonoBehaviour
{
    // Parameters for Flocking Behavior
    public float maxSpeed = 5f;         // Maximum speed
    public float maxForce = 1f;        // Maximum steering force

    // Radius for behaviors
    public float separationRadius = 2f;  // radius of influence for separation force
    public float alignmentRadius = 4f;  // radius of influence for aligning boids
    public float cohesionRadius = 4f;  // radius of influence for creating flocks of boids
    public bool disableYvelocity = true;  // just for visual purposes, can be set to false

    // Internal properties
    private Vector3 velocity;
    private Vector3 repulsionForce;
    private float repulsionForceDuration = 0;

    // Reference to all boids
    private static List<BoidBehaviour> allBoids = new List<BoidBehaviour>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize velocity with a random direction
        float yVelocity =  (disableYvelocity) ? 0 : Random.Range(-maxSpeed, maxSpeed);
        velocity = new Vector3(Random.Range(-maxSpeed, maxSpeed), yVelocity, Random.Range(-maxSpeed, maxSpeed)).normalized * maxSpeed;

        // Static list containing all generated boids
        allBoids.Add(this);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 acceleration;
        if (this.repulsionForceDuration > 0)  // if active repulsion force bypass the 3 rules (separation, alignment, cohesion)
        {
            acceleration = repulsionForce;
            this.repulsionForceDuration -= Time.deltaTime;
            float influence = (this.repulsionForceDuration > 0.5) ? this.repulsionForceDuration : 0.5f;
            velocity = maxSpeed * influence * acceleration.normalized;
        }
        else
        {
            // Apply the three flocking rules
            Vector3 separation = ComputeSeparation();
            Vector3 alignment = ComputeAlignment();
            Vector3 cohesion = ComputeCohesion();

            // Combine the forces with weights
            float separationWeight = 4.5f;  // Stronger repulsion to avoid collision as much possible
            float alignmentWeight = 2f;
            float cohesionWeight = 2f;

            acceleration = separation * separationWeight +
                           alignment * alignmentWeight +
                           cohesion * cohesionWeight;
        }
        float elapsedTime = Time.deltaTime;

        // Update velocity & position based on computed acceleration
        velocity += acceleration * elapsedTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);
        transform.position += velocity * elapsedTime;

        // Make the boid face the direction of its velocity
        if (velocity != Vector3.zero)
            transform.forward = velocity;
    }

    private Vector3 ComputeSeparation()
    {
        // function that computes force of separation based on boids within the radius of influence (distance-based rule)
        Vector3 force = Vector3.zero;
        int count = 0;
        // loop through all object to find those within the range of influence
        foreach (BoidBehaviour other in allBoids)
        {
            if (other == this) continue;  // if searched object is itself skip

            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance < separationRadius && distance > 0)
            {
                Vector3 D = transform.position - other.transform.position;
                force += D.normalized / distance; // higher values in axis where objects are closer
                count++;
            }
        }
        if (count > 0) force /= count; // Average the forces
        return force.normalized * maxForce;
    }

    private Vector3 ComputeAlignment()
    {
        // function that computes the force to align boids with the direction of their flock (velocity-based rule)
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;
        foreach (BoidBehaviour other in allBoids)
        {
            if (other == this) continue;  // if searched object is itself skip
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance < alignmentRadius)  // average all velocities of boids within the radius of influence
            {
                averageVelocity += other.velocity;
                count++;
            }
        }
        if (count > 0)
        {
            averageVelocity /= count;
            Vector3 dir = averageVelocity - velocity;
            return Vector3.ClampMagnitude(dir, maxForce);
        }
        return Vector3.zero;  // if no boid in given radius return zero force
    }

    private Vector3 ComputeCohesion()
    {
        // function responsible to gather boids in flocks (position-based rule)
        Vector3 COM = Vector3.zero;
        int count = 0;
        foreach (BoidBehaviour other in allBoids)  // collect the position of all boids within radius of influence and find COM
        {
            if (other == this) continue;
            float distance = Vector3.Distance(transform.position, other.transform.position);
            if (distance < cohesionRadius)
            {
                COM += other.transform.position;  // found within range, accumulate its position
                count++;
            }
        }
        if (count > 0)
        {
            COM /= count;
            Vector3 dirCOM = COM - transform.position;  // 
            return Vector3.ClampMagnitude(dirCOM, maxForce);
        }
        return Vector3.zero;
    }

    public void ApplyRepulsionForce(Vector3 currentPos, Vector3 forcePosition, float strength, float radius)
    {
        Vector3 D = currentPos - forcePosition;  // position of boid - click position
        float distance = D.magnitude;  // length of D vector
        float influence = (distance < radius) ? 1 - distance / radius : 0f;
        // apply repulsion force
        this.repulsionForce = influence * strength * D.normalized;
        //Vector3 acceleration = repulsionForce; // Apply the repulsion force to the boid's acceleration
        this.repulsionForceDuration = 1f;
    }
}
