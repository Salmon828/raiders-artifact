using UnityEngine;
using UnityEngine.UIElements;

public class SawChain : MonoBehaviour
{
    [SerializeField] private float speedMult = 2f; // Multiplies how fast the saw swings
    [SerializeField] private float upperBound = 50f; // Upper rotation limit in degrees
    [SerializeField] private float lowerBound = -60f; // Lower rotation limit in degrees
    private float currentRotation;
    [SerializeField] private int direction = 1;

    private float center;
    private float magnitude;
    [SerializeField] private float piOffset = 1;

    private void Awake()
    {
        center = (upperBound + lowerBound) / 2;
        magnitude = upperBound - center;
    }

    // Update is called once per frame
    void Update()
    {
        currentRotation = center + magnitude * Mathf.Sin((Time.time * speedMult) + piOffset * Mathf.PI);
        transform.localRotation = Quaternion.Euler(currentRotation, 0f, 0f);
    }
}
