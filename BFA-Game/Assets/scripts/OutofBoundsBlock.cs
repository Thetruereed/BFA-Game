using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBoundsBlock : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private void Start()
    {
        // Store the original position and rotation of the game object
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider belongs to the game object that needs to be respawned
        if (other.CompareTag("RespawnableObject"))
        {
            // Respawn the game object to its original position and rotation
            RespawnObject(other.gameObject);
        }
    }

    private void RespawnObject(GameObject objToRespawn)
    {
        // Set the object's position and rotation to the original values
        objToRespawn.transform.position = originalPosition;
        objToRespawn.transform.rotation = originalRotation;

        // If the object has a Rigidbody, reset its velocity and angular velocity
        Rigidbody objRigidbody = objToRespawn.GetComponent<Rigidbody>();
        if (objRigidbody != null)
        {
            objRigidbody.velocity = Vector3.zero;
            objRigidbody.angularVelocity = Vector3.zero;
        }

        // Add any other custom respawn logic you might need for your specific game object
    }
}