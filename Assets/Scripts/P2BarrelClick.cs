using UnityEngine;
using UnityEngine.InputSystem;

public class P2BarrelClick : MonoBehaviour
{
    // Detects player clicks on barrels + plays knock sound
    public int barrelID;
    public AudioSource knockSound;  // for barrel knocking sound
    public float interactDistance = 5f; // like pickup.cs (how far to interact)
    public TMPro.TextMeshPro textLabel;

    private Camera playerCam;

    private P2BarrelPuzzManager p2Manager;

    void Start()
    {
        p2Manager = FindObjectOfType<P2BarrelPuzzManager>();
        playerCam = Camera.main;
        textLabel.text = barrelID.ToString();
    }

    void Update()
    {
        // if (Mouse.current.leftButton.wasPressedThisFrame)
        if(Keyboard.current.eKey.wasPressedThisFrame)
        {
            Ray r = playerCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            RaycastHit hit;

            if(Physics.Raycast(r, out hit, interactDistance))
            {
                if(hit.collider.gameObject == gameObject)
                {
                    Knock();
                }
            }
        }
    }

    void Knock()
    {
        knockSound.Play();
        Debug.Log("BARRELID " + barrelID + " was clicked!");
        p2Manager.BarrelClicked(barrelID);
    }

}
