using UnityEngine;

// Takes care of player interaction with the vehicle trigger zones
// I've attached this to a GameObject with a Collider (set to trigger) to define interaction areas in the game
public class CarTrigger : MonoBehaviour
{
    [Header("Player Reference")]
    [Tooltip("Reference to the player's First Person Controller component")]
    public FirstPersonController playerController;

    // This is specifically Called when another collider enters this trigger zone
    // allows vehicle for the player
    private void OnTriggerEnter(Collider other)
    {
        // This is what checks if the entering object is the player using tag comparison located at the top of the inspector
        if (other.CompareTag("Player"))
        {
            // Enables car interaction in the player controller
            playerController.EnableCarInteraction(true);
        }
    }

    // Called when another collider exits this trigger zone, to define when the player leaves the zone that lets them interact with the car
    // Disabling the vehicle interaction for the player
    private void OnTriggerExit(Collider other)
    {
        // Verify the exiting object is the player
        if (other.CompareTag("Player"))
        {
            // Disable car interaction in the player controller
            playerController.EnableCarInteraction(false);
        }
    }
}