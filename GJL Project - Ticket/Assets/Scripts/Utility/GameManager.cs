using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public delegate void ResetAction();
    public static event ResetAction OnReset;
    public static event ResetAction OnTimeExpired;

    private int validPassengerCount;

    private int passengersServedFully, passengersServed, invalidTicketsServed, passengersLeftBehind, totalPassengersLeftBehind, stationsDeparted, complaintsCount, totalComplaintsCount;
    [SerializeField] private int requiredStations = 10;

    [SerializeField] private float timePerStation, earlyCompletionTime = 5f, stationTransitionTime = 3f;
    private float stationTimer;
    private bool timerStarted = false, gameComplete = false, PlayerOnTrain;
    [SerializeField] private Slider timeIndicator;
    [SerializeField] private Animator timeUIAC;
    [SerializeField] private TMP_Text complaintsText;

    [SerializeField] private PlayerController playerCtr;

    [SerializeField] private GameObject stationCompletePanel, mainMenuPanel, shiftCompletePanel;
    [SerializeField] private TMP_Text SCP_PassengersLeftText, SCP_ComplaintsRecievedText, SCP_StationNumberText, SftCP_PassengersServed, SftCP_Complaints, SftCP_Kidnappings, SftCP_PassengersLeft;
    [SerializeField] private Button continueButton, playButton;
    [SerializeField] private Color TextDefault, GreenText;

    [SerializeField] private AudioSource MenuAudioManager, MenuAudioSFXManager, MusicManager;
    [SerializeField] private AudioClip perfectPassengerSFX, imperfectPassengerSFX, kidnappingSFX, endOfShiftWhistle, buttonClick, TimeWarningSFX;

    private void OnEnable()
    {
        //subscribe to events
        Train.OnTrainArrival += startTimer;
        Train.OnTrainDeparture += AddStationCount;
        Train.OnTrainGone += ShowPanel;
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
        TimeCheck();
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

    public void resetValidPassengers() //delete all current passengers
    {
        validPassengerCount = 0;
        complaintsCount = 0;
        PlayerOnTrain = false;
        OnReset?.Invoke();
        MenuAudioManager.Stop();
        

    }
    public void resetGameStats()
    {
        totalPassengersLeftBehind = 0;
        passengersLeftBehind = 0;
        passengersServed = 0;
        passengersServedFully = 0;
        invalidTicketsServed = 0;
        stationsDeparted = 0;
        complaintsCount = 0;
        gameComplete = false;
        mainMenuPanel.SetActive(false);
        shiftCompletePanel.SetActive(false);
        complaintsText.gameObject.SetActive(false);
        MusicManager.Play();
    }

    private void scoreChange(int changeType)
    {
        if(changeType == 2)
        {
            //valid ticket and correct class
            passengersServedFully += 1;
            passengersServed += 1;
            MenuAudioSFXManager.PlayOneShot(perfectPassengerSFX);
        }
        else if (changeType == 1)
        {
            //valid ticket but incorrect class
            passengersServed += 1;
            complaintsCount += 1;
            totalComplaintsCount += 1;
            complaintsText.text = totalComplaintsCount.ToString();
            if (complaintsText.gameObject.activeSelf == false)
            {
                complaintsText.gameObject.SetActive(true);
            }
            MenuAudioSFXManager.PlayOneShot(imperfectPassengerSFX);
        }
        else if (changeType == 0)
        {
            //no valid ticket
            invalidTicketsServed += 1;
            MenuAudioSFXManager.PlayOneShot(kidnappingSFX);
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

    public void OnDeviceChanged(PlayerInput pi)
    {
        playerCtr.isGamepad = pi.currentControlScheme.Equals("Gamepad") ? true : false;

        //if gamepad, show gamepad UI. If not, Keyboard UI
    }

    private void TimeCheck()
    {
        if (timerStarted && stationTimer > 0)
        {
            stationTimer -= Time.deltaTime;
            timeIndicator.value = stationTimer;
        }
        else if (stationTimer <= 0 && timerStarted)
        {
            //time has expired
            timerStarted = false;
            OnTimeExpired?.Invoke();
            passengersLeftBehind = validPassengerCount;
            totalPassengersLeftBehind += passengersLeftBehind;
            timeUIAC.SetBool("Pulse", false);
            timeIndicator.gameObject.SetActive(false);
        }

        if(stationTimer <= earlyCompletionTime && timerStarted)
        {
            timeUIAC.SetBool("Pulse", true);
        }
        else
        {
            timeUIAC.SetBool("Pulse", false);
        }
    }

    private void AddStationCount(bool playerCanMove, bool playerCanGrab)
    {
        stationsDeparted += 1;
        if(stationsDeparted == requiredStations)
        {
            gameComplete = true;
        }
    }

    private void ShowPanel(bool playerOnTrain)
    {
        //show station completed/game over panels
        if (!playerOnTrain)
        {
            SCP_PassengersLeftText.text = "You got left behind";
            SCP_ComplaintsRecievedText.text = complaintsCount + " Complaints Recieved";
            SCP_StationNumberText.text = "Stop " + stationsDeparted + "/" + requiredStations;
        }
        else
        {
            SCP_PassengersLeftText.text = passengersLeftBehind + " Passengers Left Behind";
            SCP_ComplaintsRecievedText.text = complaintsCount + " Complaints Recieved";
            SCP_StationNumberText.text = "Stop " + stationsDeparted + "/" + requiredStations;
        }

        stationCompletePanel.GetComponent<Animator>().SetTrigger("FadeIn");
        PlayerOnTrain = playerOnTrain;
        StartCoroutine("TransitionTimer");
    }
    private void LoadNextStation() //called by UI button/timer
    {
        if (!gameComplete && PlayerOnTrain)
        {
            //Debug.Log("loading next station");
            //load next station
            resetValidPassengers();
            
        }
        else if(!gameComplete && !PlayerOnTrain)
        {
            //game over. Back to main menu
            //Debug.Log("Game Over");
            mainMenuPanel.SetActive(true);
            playButton.Select();
            MenuAudioManager.Play();
            MusicManager.Stop();
        }
        else
        {
            //load end/score screen
            //Debug.Log("Day End");
            //set all text values before turning on panel
            SftCP_PassengersServed.text = "Passengers Served: "+passengersServed;
            SftCP_Complaints.text = "Complaints: "+totalComplaintsCount;
            if(totalComplaintsCount == 0)
            {
                SftCP_Complaints.color = GreenText;
            }
            else
            {
                SftCP_Complaints.color = TextDefault;
            }
            SftCP_Kidnappings.text = "Kidnappings: "+invalidTicketsServed;
            if (invalidTicketsServed == 0)
            {
                SftCP_Kidnappings.color = GreenText;
            }
            else
            {
                SftCP_Kidnappings.color = TextDefault;
            }
            SftCP_PassengersLeft.text = "Passengers Left Behind: "+totalPassengersLeftBehind;
            if (totalPassengersLeftBehind == 0)
            {
                SftCP_PassengersLeft.color = GreenText;
            }
            else
            {
                SftCP_PassengersLeft.color = TextDefault;
            }
            shiftCompletePanel.SetActive(true);
            continueButton.Select();
            MenuAudioManager.Play();
            MusicManager.Stop();
            MenuAudioSFXManager.PlayOneShot(endOfShiftWhistle);
            //if complaints, kidnappings, and passengers left are all 0 play confetti particles?
        }
    }
    public IEnumerator TransitionTimer()
    {
        yield return new WaitForSeconds(stationTransitionTime - 0.5f);
        LoadNextStation();
        yield return new WaitForSeconds(0.5f);
        stationCompletePanel.GetComponent<Animator>().SetTrigger("FadeOut");
    }

    public void FullscreenToggle(bool status)
    {
        Screen.fullScreen = status;
    }
    public void quitGame()
    {
        Screen.fullScreen = false;
    }

    public void mainMenu()
    {
        mainMenuPanel.SetActive(true);
        playButton.Select();
        
    }
    public void ButtonSFX()
    {
        MenuAudioSFXManager.PlayOneShot(buttonClick);
    }
}
