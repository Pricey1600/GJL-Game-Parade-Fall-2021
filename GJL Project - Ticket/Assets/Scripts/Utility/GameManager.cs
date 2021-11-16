using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public delegate void ResetAction();
    public static event ResetAction OnReset;

    private int validPassengerCount;

    private void OnEnable()
    {
        //subscribe to events
        Ticket.OnPassengerSpawn += addValidPassenger;
    }

    private void OnDisable()
    {
        Ticket.OnPassengerSpawn -= addValidPassenger;
    }

    private void addValidPassenger() //adds to the passenger count
    {
        validPassengerCount += 1;
    }

    private void subtractValidPassenger() //subtracts from the passenger count
    {
        validPassengerCount -= 1;
    }

    private bool passengersRemainingCheck() //check if there are any valid passengers left
    {
        if(validPassengerCount > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void resetValidPassengers() //delete all current passengers
    {
        validPassengerCount = 0;
        OnReset?.Invoke();
    }
}
