using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class SeekBehaviour : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector3 initialVelocity = new Vector3 (0.3f, 0f, -1.0f);
    public float maxAcceleration = 3.5f;
    public float maxVelocity = 2.5f;

    private Vector3 velocity;
    private bool isPaused = false;
    public bool pauseOnTouch = true;

    void Start()
    {
        // collect current position and velocity
        this.velocity = initialVelocity;
    }

    // Update is called once per frame
    void Update()
    {
        float timeElapsed = Time.deltaTime;
        GameObject targetParticle = GameObject.Find("TargetParticle");
        if (targetParticle != null) 
        { 
            Vector3 targetPosition = targetParticle.transform.position;
            Vector3 acceleration = SeekTarget(targetPosition);
            this.gameObject.transform.position += this.velocity * timeElapsed;
            this.velocity += acceleration * timeElapsed;
            // bound also the velocity of seek particle (or else we have an orbit like behaviour)
            this.velocity.x = Mathf.Clamp(this.velocity.x, -maxVelocity, maxVelocity);
            this.velocity.y = Mathf.Clamp(this.velocity.y, -maxVelocity, maxVelocity);
            this.velocity.z = Mathf.Clamp(this.velocity.z, -maxVelocity, maxVelocity);
            if (this.pauseOnTouch) this.PauseOneTouch(ref targetParticle);
        }
        else
        { 
            Debug.LogError("TargetParticle not found");
        }
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
        else if (distance > 2e-1 && this.isPaused) {
            this.isPaused = false;
        }
    }

    private Vector3 SeekTarget(Vector3 targetPosition)
    {
        Vector3 characterPosition = this.gameObject.transform.position;
        Vector3 diff = targetPosition - characterPosition;
        float magn = Mathf.Sqrt(diff[0]* diff[0] + diff[1] * diff[1] + diff[2] * diff[2]);
        Vector3 acceleration = (diff / magn) * this.maxAcceleration;
        return acceleration;
    }
}
