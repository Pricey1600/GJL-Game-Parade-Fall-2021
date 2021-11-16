using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public delegate void ResetAction();
    public static event ResetAction OnReset;
    public static event ResetAction OnTimeExpired;

    private int validPassengerCount;

    private int passengersServedFully, passengersServed, invalidTicketsServed, passengersLeftBehind, totalPassengersLeftBehind;

    [SerializeField] private float timePerStation, earlyCompletionTime = 5f;
    private float stationTimer;
    private bool timerStarted = false;
    [SerializeField] private Slider timeIndicator;

    //[SerializeField] private GameObject trainObj;

    private void OnEnable()
    {
        //subscribe to events
        Train.OnTrainArrival += startTimer;
        Ticket.OnPassengerSpawn += addValidPassenger;
        DropCheck.OnPassengerLoaded += subtractValidPassenger;
        DropPoint.OnScoreChange += scoreChange;
    }

    private void OnDisable()
    {
        Train.OnTrainArrival -= startTimer;
        Ticket.OnPassengerSpawn -= addValidPassenger;
        DropCheck.OnPassengerLoaded -= subtractValidPassenger;
        DropPoint.OnScoreChange -= scoreChange;
    }

    private void Update()
    {
        if(timerStarted && stationTimer > 0)
        {
            stationTimer -= Time.deltaTime;
            timeIndicator.value = stationTimer;
        }
        else if(stationTimer <= 0 && timerStarted)
        {
            //time has expired
            timerStarted = false;
            OnTimeExpired?.Invoke();
            passengersLeftBehind = validPassengerCount;
            totalPassengersLeftBehind += passengersLeftBehind;
            timeIndicator.gameObject.SetActive(false);
        }
    }

    private void addValidPassenger() //adds to the passenger count
    {
        validPassengerCount += 1;
    }

    private void subtractValidPassenger() //subtracts from the passenger count
    {
        validPassengerCount -= 1;

        if (!passengersRemainingCheck())
        {
            //set time limit to a 'departing' value if not already lower than it
            if(stationTimer > earlyCompletionTime)
            {
                stationTimer = earlyCompletionTime;
            }
            else
            {
                return;
            }
        }
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
        if(validPassengerCount > 0)
        {
            passengersLeftBehind += validPassengerCount;
        }
        validPassengerCount = 0;
        OnReset?.Invoke();
    }

    private void scoreChange(int changeType)
    {
        if(changeType == 2)
        {
            //valid ticket and correct class
            passengersServedFully += 1;
            passengersServed += 1;
        }
        else if (changeType == 1)
        {
            //valid ticket but incorrect class
            passengersServed += 1;
        }
        else if (changeType == 0)
        {
            //no valid ticket
            invalidTicketsServed += 1;
        }
    }

    private void startTimer()
    {
        stationTimer = timePerStation;
        timeIndicator.maxValue = timePerStation;
        timeIndicator.value = timePerStation;
        timeIndicator.gameObject.SetActive(true);
        timerStarted = true;
        passengersLeftBehind = 0;
    }

}
