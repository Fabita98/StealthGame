using System.Collections;
using UnityEngine;

public class StickController : MonoBehaviour
{
    public float detectionRange = 3.0f; // Maximum distance to detect an enemy
    public LayerMask enemyLayer; // The Layer that contains enemies to be destroyed
    public float activationTime = 2.0f; // Time needed to hold the trigger

    private bool triggerHeld = false;
    private float activationTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        // Check if the right trigger is being pressed
        if (OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
        {
            if (!triggerHeld)
            {
                // Start the countdown for activation
                triggerHeld = true;
                activationTimer = 0f;
            }

            // Increase the timer while the trigger is held
            activationTimer += Time.deltaTime;

            // If the trigger has been held for longer than activationTime, check for enemies
            if (activationTimer >= activationTime)
            {
                CheckForEnemyInFront();
                triggerHeld = false; // Reset trigger check
            }
        }
        else
        {
            // Reset the timer if the trigger is released
            triggerHeld = false;
            activationTimer = 0f;
        }
    }

    void CheckForEnemyInFront()
    {
        // Get the stick's position and forward direction
        Vector3 stickPosition = transform.position;
        Vector3 stickDirection = transform.forward;

        // Perform a Raycast to check if there's an enemy in front of the stick
        RaycastHit hit;
        if (Physics.Raycast(stickPosition, stickDirection, out hit, detectionRange, enemyLayer))
        {
            // If the Raycast hits an object in the enemy layer, destroy it
            GameObject enemy = hit.collider.gameObject;
            Debug.Log("Enemy detected and destroyed: " + enemy.name);
            Destroy(enemy); // Destroy the enemy
        }
        else
        {
            Debug.Log("No enemy detected in front.");
        }
    }

    // Method to visualize the detection ray in Unity's editor (useful for testing the detection range)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * detectionRange);
    }
}
