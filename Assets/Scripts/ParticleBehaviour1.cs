using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ParticleBehaviour1 : MonoBehaviour
{
    public float maxVelocity = 12f;

    private Vector3 velocity;
    private Vector3 windForce;
    private Vector3 gForce = new Vector3(0, -9.81f, 0);  // gravity force (applied only in y-axis)
    private float epsilon;
    private Bounds bounds;
    private Vector3 totalForce;
    private Vector3 prevPosition;
    private SupervisorBehaviour1 supervisor;
    float scale;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Reference to Supervisor to get bounds and epsilon
        this.supervisor = FindFirstObjectByType<SupervisorBehaviour1>();
        this.bounds = supervisor.bounds;
        this.epsilon = supervisor.epsilon;
        this.scale = this.gameObject.transform.localScale.x;
        // get a random position
        Vector3 randomPosition = new Vector3(Random.Range(bounds.min.x + scale, bounds.max.x - scale), Random.Range(bounds.min.y + scale, bounds.max.y - scale), Random.Range(bounds.min.z + scale, bounds.max.z - scale));
        this.gameObject.transform.position = randomPosition;
        // get a random velocity
        velocity = new Vector3(Random.Range(-1.0f, 1.0f) * maxVelocity, Random.Range(-1.0f, 1.0f) * maxVelocity, Random.Range(-1.0f, 1.0f) * maxVelocity);
        // get wind force
        this.windForce = supervisor.vWind * supervisor.kWind;
    }

    // Update is called once per frame
    void Update()
    {
        // compute total force
        this.totalForce = this.windForce + this.gForce;
        this.prevPosition = this.gameObject.transform.position;
        // collision detection
        CollisionHandler();
    }


    private (float, bool, bool) CalculateCollisionTime(float position, float prevPosition, float velocity, float minBound, float maxBound)
    {
        // function that calculates collision time and returns useful info
        if (position > minBound && position < maxBound) return (Mathf.Infinity, true, false); // no collision
        // v = Dx/Dt => Dt = Dx/v
        if (position >= maxBound)
        {
            return (Mathf.Abs(position - maxBound) / Mathf.Abs(velocity), false, maxBound == prevPosition);
        }
        else if (position <= minBound)
        {
            return (Mathf.Abs(minBound - position) / Mathf.Abs(velocity), false, minBound == prevPosition);
        }
        return (Mathf.Infinity, false, false);
    }


    void CollisionHandler()
    {
        if (this.gameObject == null) return;
        Vector3 position = this.gameObject.transform.position;
        // Use Euler method to update position and velocity
        // Time.deltaTime is the time interval from one frame to next frame
        float elapsedTime = Time.deltaTime;  // time elapsed from one frame to next

        while (elapsedTime > 0)  // more than one collision might occur during elapsed time! track them all!
        {
            Vector3 nextPosition = position + elapsedTime * this.velocity;  // compute next position and velocity based on remaining elapsed time
            Vector3 nextVelocity = this.velocity + elapsedTime * this.totalForce;

            // ToDo: first find minimum time > 0 and then operate with this
            var (xTime, noCollisionX, bothInBoundsX) = CalculateCollisionTime(nextPosition[0], position[0], this.velocity.x, bounds.min.x, bounds.max.x);
            if (bothInBoundsX)  // multiple positions in boundary velocity -> 0
            {
                this.velocity[0] = 0f;
                xTime = Mathf.Infinity;
            }
            var (yTime, noCollisionY, bothInBoundsY) = CalculateCollisionTime(nextPosition[1], position[1], this.velocity.y, bounds.min.y, bounds.max.y);
            if (bothInBoundsY)
            {
                this.velocity[1] = 0f;
                yTime = Mathf.Infinity;
            }
            var (zTime, noCollisionZ, bothInBoundsZ) = CalculateCollisionTime(nextPosition[2], position[2], this.velocity.z, bounds.min.z, bounds.max.z);
            if (bothInBoundsZ)
            {
                this.velocity[2] = 0f;
                zTime = Mathf.Infinity;
            }
            // check if no collision detected, normal Euler update
            if (noCollisionX && noCollisionY && noCollisionZ)  
            {
                this.gameObject.transform.position = nextPosition;
                this.velocity = nextVelocity;
                return;
            }
            // find min collision time
            float minTime = Mathf.Min(xTime, yTime, zTime);
            if (minTime==0 || minTime == Mathf.Infinity) return;  // no update required (steady state)
            // update the position to the first collision detected
            position = position + this.velocity * minTime;
            // Handle each axis that collides at the earliest collision time (support precision errors with Clamp)
            if (Mathf.Approximately(minTime, xTime))
            {
                this.velocity.x *= -supervisor.epsilon;
                position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
            }
            if (Mathf.Approximately(minTime, yTime))
            {
                this.velocity.y *= -supervisor.epsilon;
                position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
            }
            if (Mathf.Approximately(minTime, zTime))
            {
                this.velocity.z *= -supervisor.epsilon;
                position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);
            }
            // Update remaining time by subtracting the time taken to reach the collision
            elapsedTime = elapsedTime - minTime;
        }
        // Final position clamping to prevent particles from escaping due to precision errors
        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
        position.z = Mathf.Clamp(position.z, bounds.min.z, bounds.max.z);
        this.gameObject.transform.position = position;
    }
}
