using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Checkpoint can only be activated once
    private bool isActivated = false;

    // Respawn position
    public Transform spawnPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore if already activated
        if (isActivated)
            return;

        // Check if the player entered
        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();

        if (player != null)
        {
            isActivated = true;

            // Save checkpoint position
            player.SetCheckpoint(spawnPoint.position);

            UnityEngine.Debug.Log("Checkpoint activated!");
        }
    }
}
