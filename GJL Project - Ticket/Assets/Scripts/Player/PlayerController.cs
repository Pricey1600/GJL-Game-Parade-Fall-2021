using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera aimCamera;
    [SerializeField] private Animator playerGFXAC;

    [SerializeField] private Rigidbody RB;
    [SerializeField] private Transform grabPoint;
    [SerializeField] private Grabber grabber;
    private bool validGrab, grabbing;
    private GameObject grabTarget;
    private Animator grabTargetAC;
    [SerializeField] private LayerMask whatIsPassenger;

    public bool canMove = false, canGrab = false;
    private bool isMoving;
    [SerializeField] private float moveSpeed, controllerDeadzone = 0.1f, gamepadRotationSmoothing = 1000f, keyboardRotationSmoothing = 5000f;
    private float inputX, inputZ, aimX, aimY;
    Vector3 mousePos, mousePosRaw, prevMousePos;
    [SerializeField] Vector3 throwForce;

    [SerializeField] public bool isGamepad;
    private bool rightStickOnRoation, mouseStatic;
    private float mouseStaticTimer;

    [SerializeField] private Train TrainScript;

    [SerializeField] private AudioSource playerAudio;
    [SerializeField] private AudioClip grabSFX;

    private void OnEnable()
    {
        Train.OnTrainArrival += TrainArrivalChecks;
        Train.OnTrainDeparture += TrainDepartedChecks;
    }
    private void OnDisable()
    {
        Train.OnTrainArrival += TrainArrivalChecks;
        Train.OnTrainDeparture -= TrainDepartedChecks;
    }
    private void FixedUpdate()
    {
        if (canMove)
        {
            RB.velocity = new Vector3(inputX, 0, inputZ) * moveSpeed;
        }
        else if(canGrab)
        {
            canGrab = false;
        }

        if (!canMove && RB.velocity != Vector3.zero)
        {
            inputX = 0;
            inputZ = 0;
            RB.velocity = Vector3.zero;
        }

        #region Rotation
        if (canMove)
        {
            if (isMoving && !rightStickOnRoation && isGamepad)
            {
                Vector3 playerDirection = Vector3.right * inputX + Vector3.forward * inputZ;
                if (playerDirection.sqrMagnitude > 0.0f)
                {
                    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, gamepadRotationSmoothing * Time.deltaTime);
                }
            }
            else if ((Mathf.Abs(aimX) > controllerDeadzone || Mathf.Abs(aimY) > controllerDeadzone) && isGamepad)
            {
                Vector3 playerDirection = Vector3.right * aimX + Vector3.forward * aimY;
                if (playerDirection.sqrMagnitude > 0.0f)
                {
                    Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, gamepadRotationSmoothing * Time.deltaTime);
                }
            }
            else if (!isGamepad)
            {
                if (prevMousePos == mousePosRaw)
                {
                    if (!mouseStatic)
                    {
                        //Debug.Log("mouse is static");
                        mouseStatic = true;
                        mouseStaticTimer = 2f;
                    }

                }
                else
                {
                    mouseStatic = false;
                }

                if (!mouseStatic || mouseStaticTimer > 0)
                {
                    Ray ray = aimCamera.ScreenPointToRay(mousePosRaw);
                    Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
                    float rayDistance;

                    if (groundPlane.Raycast(ray, out rayDistance))
                    {
                        mousePos = ray.GetPoint(rayDistance);

                    }
                    LookAt(mousePos);
                }
                else if (mouseStatic && mouseStaticTimer <= 0)
                {
                    Vector3 playerDirection = Vector3.right * inputX + Vector3.forward * inputZ;
                    if (playerDirection.sqrMagnitude > 0.0f)
                    {
                        Quaternion newRotation = Quaternion.LookRotation(playerDirection, Vector3.up);
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, newRotation, keyboardRotationSmoothing * Time.deltaTime);
                    }
                }

            }
        }
        
        #endregion

        validGrab = Physics.CheckSphere(grabPoint.position, .5f, whatIsPassenger);
        prevMousePos = mousePosRaw;
    }

    private void Update()
    {
        if(mouseStaticTimer > 0)
        {
            mouseStaticTimer -= Time.deltaTime;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        inputX = context.ReadValue<Vector2>().x;
        inputZ = context.ReadValue<Vector2>().y;

        if(inputX != 0 || inputZ != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;
        }

        
    }

    public void Rotate(InputAction.CallbackContext context)
    {
        //Debug.Log("Rotate method called by: " + context.control);
        if (isGamepad)
        {
            aimX = context.ReadValue<Vector2>().x;
            aimY = context.ReadValue<Vector2>().y;
            rightStickOnRoation = true;

            if (aimX == 0 && aimY == 0)
            {
                rightStickOnRoation = false;

            }
        }
        else
        {
            
            mousePosRaw = context.ReadValue<Vector2>();
            

        }

    }

    private void LookAt(Vector3 lookPoint)
    {
        Vector3 heightCorrectedPoint = new Vector3(lookPoint.x, transform.position.y, lookPoint.z);
        transform.LookAt(heightCorrectedPoint);
    }

    public void Grab(InputAction.CallbackContext context)
    {
        if (!context.started || !canGrab)
        {
            return;
        }

        if (validGrab && !grabbing)
        {
            grabbing = true;
            grabTarget = grabber.grabTarget;
            grabTarget.transform.SetParent(this.gameObject.transform);

            grabTarget.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 1, this.gameObject.transform.position.z);
            grabTarget.transform.localRotation = Quaternion.Euler(270, 0, 90);

            grabTargetAC = grabTarget.GetComponentInChildren<Animator>();

            if(grabTarget.GetComponent<NavMeshAgent>() != null)
            {
                grabTarget.GetComponent<NavMeshAgent>().enabled = false;
                grabTarget.GetComponent<RandomWalking>().enabled = false;
                playerGFXAC.SetBool("isGrabbing", true);
                grabTargetAC.SetBool("isGrabbed", true);
            }
            else
            {
                playerGFXAC.SetBool("GrabbingThrowable", true);
            }
            
            grabTarget.GetComponent<Collider>().enabled = false;
            grabTarget.GetComponent<Rigidbody>().isKinematic = true;

            playerAudio.PlayOneShot(grabSFX);
            
        }
        else if (grabbing)
        {
            grabbing = false;

            if(grabTarget != null && grabTarget.GetComponent<NavMeshAgent>() != null)
            {
                grabTarget.transform.parent = null;
                grabTarget.transform.position = new Vector3(grabPoint.transform.position.x, grabPoint.transform.position.y, grabPoint.transform.position.z);
                grabTarget.transform.rotation = Quaternion.Euler(this.gameObject.transform.rotation.x, this.gameObject.transform.rotation.y, this.gameObject.transform.rotation.z);

                grabTarget.GetComponent<Collider>().enabled = true;
                grabTarget.GetComponent<Rigidbody>().isKinematic = false;
                playerGFXAC.SetBool("isGrabbing", false);
                grabTargetAC.SetBool("isGrabbed", false);
            }
            else
            {
                grabTarget.transform.parent = null;
                grabTarget.GetComponent<Collider>().enabled = true;
                grabTarget.GetComponent<Rigidbody>().isKinematic = false;
                grabTarget.GetComponent<Rigidbody>().AddRelativeForce(throwForce, ForceMode.Impulse);
                playerGFXAC.SetBool("GrabbingThrowable", false);
            }

            
        }
    }

    private void TrainDepartedChecks(bool canMoveSet, bool canGrabSet)
    {
        if (!canGrabSet && !canMoveSet && grabbing)
        {
            if(grabTarget.GetComponent<Ticket>() != null)
            {
                grabTarget.GetComponent<Ticket>().destroyPassenger();
            }
            else
            {
                Destroy(grabTarget);
            }
            
            grabTarget = null;
            grabTargetAC = null;
        }
        
        canMove = canMoveSet;
        canGrab = canGrabSet;
        playerGFXAC.SetBool("isGrabbing", false);
        playerGFXAC.SetBool("GrabbingThrowable", false);
    }

    private void TrainArrivalChecks()
    {
        canMove = true;
        canGrab = true;
        grabbing = false;
    }

    public void OnArrival()
    {
        TrainScript.trainArrived();
        this.GetComponent<Animator>().enabled = false;
    }



}
