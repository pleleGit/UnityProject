using Unity.VisualScripting;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif

public class TargetBehaviour : MonoBehaviour
{
    public Vector3 velocity = new Vector3(0f, 0f, 0.8f);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float timeElapsed = Time.deltaTime;  // elapsed time from one frame to next
        this.gameObject.transform.position = transform.position + velocity * timeElapsed;
        if (transform.position.x > 10 || transform.position.y > 10 || transform.position.z > 10)
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;  // ensure that is is called only in unity editor
#endif
        }
    }
}
