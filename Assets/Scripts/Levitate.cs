using System.Collections;
using UnityEngine;
public class Levitate : MonoBehaviour
{
    public float moveSpeed = 15f;
    
    public float effectDuration = 5f; // How long before object reverts
    private Material originalMaterial;
    private Vector3 originalScale; 
    private Rigidbody rb;
    private Vector3 lastFrameVelocity;
    public float maxSpeed = 10f; // stop object from spazzing as bounce keeps multiplying speed
    private MaterialManager materialManager;
    private Elevator elevator;

    void Start() 
    {
        rb = GetComponent<Rigidbody>();

        // Handle material change
        if (!TryGetComponent<MaterialManager>(out materialManager))
        {
            materialManager = gameObject.AddComponent<MaterialManager>();
        }
        materialManager.AddEffect(Resources.Load<Material>("Travis/Levitate"), 1f);

        // If levitate is attached to an elevator, trigger it to change floors
        if (TryGetComponent<Elevator>(out elevator))
        {
            elevator.ToggleFloor();
        }

        StartCoroutine(undoLevitateCoroutine());
    }

    private void FixedUpdate()
    {
        if (elevator != null)
        {
            return;
        }

        // Apply a continuous upward force
        rb.AddForce(Vector3.up * moveSpeed);

        // limit object speed
        lastFrameVelocity = rb.linearVelocity;
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }


    private IEnumerator undoLevitateCoroutine()
    {
        // wait 
        yield return new WaitForSeconds(effectDuration);

        if (elevator != null)
        {
            // lower elevator back down
            elevator.ToggleFloor();
        }

        // undo material change
        materialManager.RemoveEffect(Resources.Load<Material>("Travis/Levitate"), 1f);

        // remove levitate script
        Destroy(GetComponent<Levitate>());
    }


}
