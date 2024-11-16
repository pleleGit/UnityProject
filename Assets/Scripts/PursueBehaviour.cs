using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class PursueBehaviour : MonoBehaviour
{
    public Vector3 initialVelocity = new Vector3(-0.3f, 0f, -1f);
    public float maxAcceleration = 3.5f;
    public float maxVelocity = 2.5f;  // same for each axis
    public float maxTime = 2f;  // maximum allowed time to reach current target

    private Vector3 velocity;
    private bool isPaused = false;
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
            //Vector3 targetPosition = targetParticle.transform.position;
            Vector3 acceleration = PursueTarget(targetParticle);
            this.gameObject.transform.position += this.velocity * timeElapsed;
            this.velocity += acceleration * timeElapsed;
            // bound also the velocity of seek particle (or else we have an orbit like behaviour)
            this.velocity.x = Mathf.Clamp(this.velocity.x, -maxVelocity, maxVelocity);
            this.velocity.y = Mathf.Clamp(this.velocity.y, -maxVelocity, maxVelocity);
            this.velocity.z = Mathf.Clamp(this.velocity.z, -maxVelocity, maxVelocity);
            this.PauseOneTouch(ref targetParticle);
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
        else if (distance > 2e-1 && this.isPaused)
        {
            this.isPaused = false;
        }
    }

    private Vector3 SeekTarget(Vector3 targetPosition)
    {
        Vector3 characterPosition = this.gameObject.transform.position;
        Vector3 diff = targetPosition - characterPosition;
        float magn = Mathf.Sqrt(diff[0] * diff[0] + diff[1] * diff[1] + diff[2] * diff[2]);
        Vector3 acceleration = (diff / magn) * this.maxAcceleration;
        return acceleration;
    }

    private Vector3 PursueTarget(GameObject target)
    {
        TargetBehaviour targetScript = target.GetComponent<TargetBehaviour>();  // to collect target's velocity
        
        Vector3 characterPosition = this.gameObject.transform.position;

        float distance = Vector3.Distance(target.transform.position, characterPosition);
        float time = distance / this.maxVelocity;
        if (time > maxTime) time = maxTime;
        Vector3 predictedPosition = target.transform.position + targetScript.velocity * time;
        Vector3 acceleration = SeekTarget(predictedPosition);
        return acceleration;
    }
}
