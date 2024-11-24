using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ParticleBehaviour2 : MonoBehaviour
{
    public float maxVelocity = 12f;
    public float maxRepulsionForce = 30f; // upper limit for repulsion force


    private Vector3 velocity;
    private SupervisorBehaviour2 supervisor;  // to collect center, radius etc
    private Vector3 center;
    private float radius;

    private const float repulsionConstant = 2.0f; // Adjust this constant to control repulsion strength
    private const float minVelocityThreshold = 0.01f; // Minimum velocity threshold

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Reference to Supervisor to get center and radius
        this.supervisor = FindFirstObjectByType<SupervisorBehaviour2>();
        this.center = supervisor.center;
        this.radius = supervisor.radius;
        // Start position at sphere center
        this.gameObject.transform.position = center;  // all particles start for sphere's center
        // Random velocity
        velocity = new Vector3(
            Random.Range(-0.2f, 0.2f) * maxVelocity,
            Random.Range(-0.2f, 0.2f) * maxVelocity,
            Random.Range(-0.2f, 0.2f) * maxVelocity
        );
        // velocity = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = transform.position;
        float elapsedTime = Time.deltaTime;
        // Compute cumulative repulsion force from all neighboring particles
        Vector3 repulsionForce = ComputeRepulsionForce();
        while (elapsedTime > 0)  // examine if collision occured (multiple collision can be handled during elapsed time)
        {
            // Add repulsion to velocity, capped at max force
            velocity += repulsionForce * elapsedTime;  // v' = v + a*Dt
            // Solve for next position
            Vector3 nextPosition = position + velocity * elapsedTime;  // p' = p + v*Dt
            // Solve for collision with sphere
            float collisionTime = CalculateCollisionTime(position, nextPosition);
            if (collisionTime < elapsedTime)
            {
                // update position to collision point
                position += velocity * collisionTime;
                // reflect velocity at collision point  V' = V - 2(V.N)N -> same as Vector3.Reflect method
                Vector3 normal = (position - center).normalized;  // normal vector from center to position
                velocity = velocity - 2 * Vector3.Dot(velocity, normal) * normal;  // project velocity to normal and subtract velocity from x2 components to normal vector
                // clamp position to the boundaries of sphere (remove a tiny value to avoid numerical precision errors)
                position = center + normal * (radius - 1e-5f);
                // reduce remaining time
                elapsedTime -= collisionTime;
            }
            else
            {
                // no collision detected in elapsed time
                position = nextPosition;
                elapsedTime = 0;
            }
        }
        // Update position
        transform.position = position;
    }


    Vector3 ComputeRepulsionForce()
    {
        Vector3 totalRepulsion = Vector3.zero;  // initialize to zero
        foreach (GameObject particle in supervisor.getGameObjects())
        {
            if (particle == this.gameObject) continue; // Skip examining with itself
            Vector3 direction = transform.position - particle.transform.position;  // vector from neighbor position to itself
            float distance = direction.magnitude + 1e-6f; // add a very small number to ensure that we avoid division with zero
            if (distance > 0) // Avoid division by zero
            {
                // Force inversely proportional to distance, capped
                totalRepulsion += direction.normalized * Mathf.Min(repulsionConstant / (distance * distance), maxRepulsionForce);
            }
        }
        return Vector3.ClampMagnitude(totalRepulsion, maxRepulsionForce);  // threshold to avoid extremely high values for computed force
    }

    float CalculateCollisionTime(Vector3 position, Vector3 nextPosition)
    {
        Vector3 relPos = position - center;
        // Solve quadratic equation: (v*t + p)·(v*t + p) = r^2
        // (v.v)t^2 + 2(v.p)t + (p.p) - r^2 = 0
        float a = Vector3.Dot(velocity, velocity);
        float b = 2 * Vector3.Dot(velocity, relPos);
        float c = Vector3.Dot(relPos, relPos) - radius * radius;
        float D = b * b - 4 * a * c;
        if (D < 0) return Mathf.Infinity; // No collision detected
        // find two possible solutions
        float sqrtD = Mathf.Sqrt(D);
        float t1 = (-b - sqrtD) / (2 * a);
        float t2 = (-b + sqrtD) / (2 * a);
        // keep the smallest positive time
        if (t1 > 0 && t2 > 0)
            return Mathf.Min(t1, t2);
        else if (t1 > 0)
            return t1;
        else if (t2 > 0)
            return t2;
        return Mathf.Infinity; // No valid collision found, return inf
    }
}
