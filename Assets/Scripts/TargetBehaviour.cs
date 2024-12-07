using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

public class TargetBehaviour : MonoBehaviour
{
    public Vector3 velocity = new Vector3(0f, 0f, 0.8f);
    public float maxVelocity = 1.5f;  // magnitude of the maximum velocity
    public bool steerOnClick = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (this.steerOnClick)
        {
            // Handle mouse click for changing velocity
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                updateVelocityOnClick();
            }
        }
        float timeElapsed = Time.deltaTime;  // elapsed time from one frame to next
        this.gameObject.transform.position = transform.position + velocity * timeElapsed;
        if (transform.position.x > 10 || transform.position.y > 10 || transform.position.z > 10)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;  // ensure that is is called only in unity editor
#endif
        }
    }

    void updateVelocityOnClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Check if the ray hits anything
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // calculate normal vector from particle to click position
            Vector3 clickPos = hitInfo.point;
            Vector3 normVec = (clickPos - transform.position).normalized;
            // update velocity based on the direction of normal vector
            this.velocity = normVec * maxVelocity;
        }
    }
}
