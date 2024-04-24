using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class EndCondition : MonoBehaviour
{
    public string importantTag = "Important"; // Tag for important objects
    public int requiredCount = 3; // Required number of important objects to trigger win condition
    public Transform teleportDestination; // Destination to teleport the player

    private int itemCount = 0;
    private Transform player;
    private bool winConditionMet = false;

    private void Update()
    {
        // Check if the win condition is already met
        if (winConditionMet)
        {
            return;
        }

        // Iterate through all interactables in the scene
        foreach (XRGrabInteractable interactable in FindObjectsOfType<XRGrabInteractable>())
        {
            // Check if the interactable is grabbed
            if (interactable.isSelected)
            {
                // Check if the grabbed object has the important tag
                GameObject grabbedObject = interactable.gameObject;
                if (grabbedObject.CompareTag(importantTag))
                {
                    itemCount++;
                    Debug.Log("Number of important objects grabbed: " + itemCount);
                }
            }
        }

        // Check if the required count is reached
        if (itemCount >= requiredCount)
        {
            // Trigger win condition
            winConditionMet = true;
            TeleportPlayer();
        }
    }

    private void TeleportPlayer()
    {
        // Teleport the XR Rig to the destination
        if (teleportDestination != null)
        {
            // Calculate the offset to maintain the relative position of the XR Rig
            Vector3 offset = teleportDestination.position - Camera.main.transform.position;
            // Teleport the XR Rig to the destination
            transform.position += offset;
        }
    }
}
