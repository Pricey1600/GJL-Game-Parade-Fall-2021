using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    public GameObject grabTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Passenger" || other.gameObject.tag == "Grabable")
        {
            grabTarget = other.gameObject;
        }

    }
}
