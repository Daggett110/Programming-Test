/* Used to move the player to another location within the level
 * */
using UnityEngine;

public class TeleportOnContact : MonoBehaviour {

    public Transform destination;       // The location we want to teleport the player to

    Transform controller, cameraRig;    // References to the character and camera rig so we may move them both to the intended location

    private void Start()
    {
        // Assign references
        controller = FindObjectOfType<HeisenballCharacterController>().transform;
        cameraRig = Camera.main.transform.parent;
    }

    private void OnTriggerEnter(Collider other)
    {
        // if we have collided with the player
        if(other.tag == "Player" && destination)
        {
            // Reset Player Position
            controller.position = destination.position;
            controller.rotation = destination.rotation;

            // I don't want the character to fly off when we get here, so let's zero out the velocity
            controller.GetComponent<Rigidbody>().velocity = Vector3.zero;

            // Don't bother moving the camera if the player wants to view the goal
            if (!Input.GetButton("View Goal"))
            {
                // Reset Camera Position
                cameraRig.position = destination.position;
                cameraRig.rotation = destination.rotation;
            }
        }
    }
}
