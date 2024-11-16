using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ArriveBehaviour : MonoBehaviour
{
    public Vector3 initialVelocity = new Vector3(1.2f, 0f, -1.5f);
    public float maxAcceleration = 5.0f;
    public float maxSpeed = 3.5f;
    public float slowRadius = 4.0f;

    private Vector3 velocity;
    private float targetRadius = -1f;
    private bool isPaused = false;
    public bool pauseOnTouch = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.velocity = initialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        float timeElapsed = Time.deltaTime;
        GameObject targetParticle = GameObject.Find("TargetParticle");
        if (targetParticle != null)
        {
            if (this.targetRadius == -1) this.targetRadius = targetParticle.transform.localScale[0]; 
            // call arrive algorithm
            Vector3 acceleration = ArriveTarget(ref targetParticle, this.targetRadius, this.slowRadius);
            this.gameObject.transform.position += this.velocity * timeElapsed;
            this.velocity += acceleration * timeElapsed;
            // bound also the velocity of seek particle (or else we have an orbit like behaviour)
            this.velocity.x = Mathf.Clamp(this.velocity.x, -maxSpeed, maxSpeed);
            this.velocity.y = Mathf.Clamp(this.velocity.y, -maxSpeed, maxSpeed);
            this.velocity.z = Mathf.Clamp(this.velocity.z, -maxSpeed, maxSpeed);
            if (this.pauseOnTouch) this.PauseOneTouch(ref targetParticle);
        }
        else
        {
            Debug.LogError("TargetParticle not found");
        }
    }

    private Vector3 ArriveTarget(ref GameObject target, float targetRadius, float slowRadius)
    {
        Vector3 diff = target.transform.position - this.gameObject.transform.position;  // difference of the two vectors
        float distance = Mathf.Sqrt(diff[0] * diff[0] + diff[1] * diff[1] + diff[2] * diff[2]);
        float time = distance / this.maxSpeed;
        float targetSpeed;  // targetSpeed is scalar (velocity magnitude) not vector! velocity is vector
        if (distance < targetRadius) return new Vector3(0f, 0f, 0f);
        if (distance > slowRadius)
        {
            targetSpeed = maxSpeed;  // if far away reach with full speed
        }
        else
        {
             targetSpeed = maxSpeed * distance / slowRadius;  // if getting closer reduce desired speed as distance decreased
        }
        Vector3 targetVelocity = (diff / distance) * targetSpeed;
        Vector3 acceleration = (targetVelocity - this.velocity) / time;
        float accelMagnitude = Mathf.Sqrt(acceleration[0] * acceleration[0] + acceleration[1] * acceleration[1] + acceleration[2] * acceleration[2]);
        if (accelMagnitude > this.maxAcceleration)
        {
            acceleration = (acceleration / accelMagnitude) * this.maxAcceleration;
        }
        return acceleration;
    }

    private void PauseOneTouch(ref GameObject target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < 2e-1 && !this.isPaused)
        {
#if UNITY_EDITOR
            EditorApplication.isPaused = true;  // ensure that is is called only in unity editor
            this.isPaused = true;
#endif
        }
        else if (distance > 2e-1 && this.isPaused)
        {
            this.isPaused = false;
        }
    }
}
