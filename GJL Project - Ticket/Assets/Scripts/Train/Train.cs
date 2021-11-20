using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    public delegate void StartAction();
    public static event StartAction OnTrainArrival;
    public delegate void EndAction(bool canMove, bool canGrab);
    public static event EndAction OnTrainDeparture;
    public delegate void ProgressAction(bool wasPlayerOnTrain);
    public static event ProgressAction OnTrainGone;


    private Collider trainCollider;
    private bool playerOnTrain;
    private Transform player;
    [SerializeField] Transform playerCarriage;

    [SerializeField] Animator TrainAC, PlayerAC;

    private void Start()
    {
        GameManager.OnTimeExpired += playerCheck;
        GameManager.OnReset += ResetTrain;
        trainCollider = this.gameObject.GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.parent = this.transform;
        player.position = new Vector3(playerCarriage.position.x, player.position.y, playerCarriage.position.z);
        player.rotation = Quaternion.Euler(0, 180, 0);

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            //player is on train
            playerOnTrain = true;
        }
        else
        {
            return;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            //player is not on train
            playerOnTrain = false;
        }
        else
        {
            return;
        }
    }

    private void ResetTrain()
    {
        //Debug.Log("Train has reset");
        PlayerAC.enabled = false;
        player.gameObject.GetComponent<PlayerController>().canMove = false;
        player.position = new Vector3(playerCarriage.position.x, player.position.y, playerCarriage.position.z);
        player.rotation = Quaternion.Euler(0, 180, 0);
        if(player.parent != this.gameObject.transform)
        {
            player.parent = this.gameObject.transform;
        }
        
        //play train arriving animation
        TrainAC.SetTrigger("TrainArrival");
    }
    public void trainArriving() //call after train has arrived at station
    {
        //unparent player from Train
        player.parent = null;
        
        
        PlayerAC.enabled = true;
        //play player step out animation
        PlayerAC.SetTrigger("PlayerStepOut");
    }
    public void trainArrived() //call after player step out animation has completed to start station timer
    {
        
        OnTrainArrival?.Invoke();
        //Debug.Log("Train has arrived");
    }

    private void playerCheck()
    {
        if (playerOnTrain)
        {
            //player did not get left behind
            //Debug.Log("player did not get left behind");

            //take away player controlls, parent to train, and play departure animation
            player.gameObject.GetComponent<PlayerController>().canMove = false;
            player.SetParent(this.transform);
            OnTrainDeparture?.Invoke(false, false);
            //animation
            TrainAC.SetTrigger("TrainDeparture");
            //Debug.Log("Train is departing");
            //station reset
        }
        else
        {
            //player got left behind
            //Debug.Log("player got left behind");

            //take away player controlls (or enable a invisable wall to stop the player getting to the train) and play departure animation
            OnTrainDeparture?.Invoke(true, false);
            TrainAC.SetTrigger("TrainDeparture");
            //game fail
        }
        
    }

    public void TrainHasLeft()
    {
        OnTrainGone?.Invoke(playerOnTrain);
        //Debug.Log("Train has left");
    }
}
