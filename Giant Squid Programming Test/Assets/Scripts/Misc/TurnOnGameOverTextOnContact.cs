/* Turns on the items in the end room when we reach it
 * */
using UnityEngine;

public class TurnOnGameOverTextOnContact : MonoBehaviour
{
    // Reference to the parent object to all the things in the end room
    public GameObject GameOverMessages;

    private void OnTriggerEnter(Collider other)
    {
        // If we make contact with the player
        if (other.tag == "Player")
        {
            // Turn objects on
            GameOverMessages.SetActive(true);
        }
    }
}
