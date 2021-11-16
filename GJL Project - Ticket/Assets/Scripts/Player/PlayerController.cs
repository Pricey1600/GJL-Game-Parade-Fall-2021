using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Rigidbody RB;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Grabber grabber;
    private bool validGrab, grabbing;
    private GameObject grabTarget, grabTargetGFX;
    [SerializeField] private LayerMask whatIsPassenger;

    public bool canMove = false;
    [SerializeField] private float moveSpeed, controllerDeadzone = 0.1f, gamepadRotationSmoothing = 1000f;
    private float inputX, inputZ, aimX, aimY;

    [SerializeField] private bool isGamepad;

    private void FixedUpdate()
    {
        if (canMove)
        {
            RB.velocity = new Vector3(inputX, 0, inputZ) * moveSpeed;
        }
        if (!canMove && RB.velocity != Vector3.zero)
        {
            inputX = 0;
            inputZ = 0;
            RB.velocity = Vector3.zero;
        }

        validGrab = Physics.CheckSphere(grabPoint.position, .2f, whatIsPassenger);
    }

    private void Update()
    {
        //if (grabbing)
        //{
        //    grabTarget.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y +1, this.gameObject.transform.position.z);
        //    grabTarget.transform.localRotation = Quaternion.Euler(270, this.gameObject.transform.rotation.y, 90);
        //}
    }

    public void Move(InputAction.CallbackContext context)
    {
        inputX = context.ReadValue<Vector2>().x;
        inputZ = context.ReadValue<Vector2>().y;

        
    }

    public void Rotate(InputAction.CallbackContext context)
    {
        if (isGamepad)
        {
            aimX = context.ReadValue<Vector2>().x;
            aimY = context.ReadValue<Vector2>().y;

            if (Mathf.Abs(aimX) > controllerDeadzone || Mathf.Abs(aimY) > controllerDeadzone)
            {
                Vector3 playerDirection = Vector3.right * aimX + Vector3.forward * aimY;
                if(playerDirection.sqrMagnitude > 0.0f)
                {
                    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, gamepadRotationSmoothing * Time.deltaTime);
                }
            }
        }

        else
        {
            Ray ray = Camera.main.ScreenPointToRay(context.ReadValue<Vector2>());
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            float rayDistance;

            if(groundPlane.Raycast(ray, out rayDistance))
            {
                Vector3 point = ray.GetPoint(rayDistance);
                LookAt(point);
            }
        }
    }

    private void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    public void Grab(InputAction.CallbackContext context)
    {
        if (!context.started)
        {
            return;
        }

        if (validGrab && !grabbing)
        {
            grabbing = true;
            grabTarget = grabber.grabTarget;
            grabTarget.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1, this.gameObject.transform.position.z);
            
            grabTarget.transform.SetParent(this.gameObject.transform);
            grabTarget.transform.localRotation = Quaternion.Euler(270, 0, 90);

            grabTarget.GetComponent<NavMeshAgent>().enabled = false;
            grabTarget.GetComponent<RandomWalking>().enabled = false;
            grabTarget.GetComponent<CapsuleCollider>().enabled = false;
            grabTarget.GetComponent<Rigidbody>().isKinematic = true;
        }
        else if (grabbing)
        {
            grabbing = false;

            grabTarget.transform.parent = null;
            grabTarget.transform.position = new Vector3(grabPoint.transform.position.x, grabPoint.transform.position.y, grabPoint.transform.position.z);
            grabTarget.transform.rotation = Quaternion.Euler(this.gameObject.transform.rotation.x, this.gameObject.transform.rotation.y, this.gameObject.transform.rotation.z);

            //check if inside train here before turning agent back on. Will cause passenger to snap back to walkable platform.

            grabTarget.GetComponent<NavMeshAgent>().enabled = true;
            grabTarget.GetComponent<RandomWalking>().enabled = true;
            grabTarget.GetComponent<CapsuleCollider>().enabled = true;
            grabTarget.GetComponent<Rigidbody>().isKinematic = false;
        }
    }

    public void OnDeviceChanged(PlayerInput pi)
    {
        isGamepad = pi.currentControlScheme.Equals("Gamepad") ? true : false;
    }
}
