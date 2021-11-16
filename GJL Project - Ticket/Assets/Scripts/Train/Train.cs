using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    public delegate void StartAction();
    public static event StartAction OnTrainArrival;

    private Collider trainCollider;
    private bool playerOnTrain;
    private Transform player;

    private void Start()
    {
        GameManager.OnTimeExpired += playerCheck;
        trainCollider = this.gameObject.GetComponent<Collider>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        trainArrived(); //remove this after animations trigger this
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

    public void trainArrived() //call after animation has completed to start station timer
    {
        OnTrainArrival?.Invoke();
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

            //station reset
        }
        else
        {
            //player got left behind
            //Debug.Log("player got left behind");

            //take away player controlls (or enable a invisable wall to stop the player getting to the train) and play departure animation

            //game fail
        }
    }
}
