using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DropCheck : MonoBehaviour
{
    public delegate void CountAction();
    public static event CountAction OnPassengerLoaded;

    public int ticketClass;

    private Ticket ticketScript;

    private void Start()
    {
        ticketScript = gameObject.GetComponent<Ticket>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Drop Point")
        {
            
            if (ticketScript.hasTicket)
            {
                //passenger has ticket
                OnPassengerLoaded?.Invoke();
                other.gameObject.SendMessage("ClassCheck", ticketScript.ticketClass);
            }
            else
            {
                //passenger hasn't got a ticket
                int ticketClass = 0;
                other.gameObject.SendMessage("ClassCheck", ticketClass);
            }
            ticketScript.destroyPassenger();
        }
        else
        {
            gameObject.GetComponent<NavMeshAgent>().enabled = true;
            gameObject.GetComponent<RandomWalking>().enabled = true;
        }
        
    }
}
