using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] Transform holdArea;
    private GameObject heldobj;
    private Rigidbody heldobjRB;

    [Header("Physics Parameters")]
    [SerializeField] private float pickupRange = 5.0f;
    [SerializeField] private float pickupForce = 150.0f;
    

    [Header("Distance Control")]
    [SerializeField] private float minDistance = 1.0f;
    [SerializeField] private float maxDistance = 10.0f;
    [SerializeField] private float distanceSpeed = 2.0f;
    private float currentDistance;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (heldobj == null)
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickupRange))
                {
                    PickupObject(hit.transform.gameObject);
                }
            }
            else
            {
                DropObject();
            }
        }

        if (heldobj != null)
        {
            MoveObject();
            ControlDistance();
        }
    }

    void MoveObject()
    {
        if (Vector3.Distance(heldobj.transform.position, holdArea.position) > 0.1f)
        {
            Vector3 moveDirection = (holdArea.position - heldobj.transform.position);
            heldobjRB.AddForce(moveDirection * pickupForce);
        }
    }

    void PickupObject(GameObject pickobj)
    {
        if (pickobj.GetComponent<Rigidbody>())
        {
            heldobjRB = pickobj.GetComponent<Rigidbody>();
            // Turns off gravity for objects so that object does not fall when picked up
            heldobjRB.useGravity = false;
            heldobjRB.drag = 10f;
            // Freeze rotation of object when picked up
            heldobjRB.constraints = RigidbodyConstraints.FreezeRotation;

            heldobjRB.transform.parent = holdArea;
            heldobj = pickobj;

            // Calculate initial distance between player and the object
            currentDistance = Vector3.Distance(transform.position, holdArea.position);
        }
    }

    void DropObject()
    {
        // Turns on gravity for objects so that object falls when dropped
        heldobjRB.useGravity = true;
        heldobjRB.drag = 1f;
        // Unfreeze rotation of object when dropped
        heldobjRB.constraints = RigidbodyConstraints.None;

        heldobj.transform.parent = null;
        heldobj = null;
    }

    void ControlDistance()
    {
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        currentDistance = Mathf.Clamp(currentDistance - scrollInput * distanceSpeed, minDistance, maxDistance);
        holdArea.position = transform.position + transform.forward * currentDistance;
    }
}