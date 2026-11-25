using UnityEngine;

public class Sawblade : MonoBehaviour
{

    [SerializeField] private float rotationSpeed = 100f; // Degrees per second

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
