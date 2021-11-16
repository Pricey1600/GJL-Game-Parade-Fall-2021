using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ticket : MonoBehaviour
{
    public bool hasTicket;
    public int ticketClass;
    [SerializeField] private TMP_Text classText;

    public delegate void CountAction();
    public static event CountAction OnPassengerSpawn;

    private void Awake()
    {
        hasTicket = ticketCheck();
        ticketClass = classCheck();

        if (hasTicket)
        {
            classText.text = ticketClass.ToString();
        }
        else
        {
            classText.enabled = false;
        }

        GameManager.OnReset += destroyPassenger;
    }

    private bool ticketCheck()
    {
        if(Random.value >= 0.5)
        {
            //trigger event
            OnPassengerSpawn?.Invoke();
            return true;
        }
        else
        {
            return false;
        }
    }

    private int classCheck()
    {
        float ranFloat = Random.Range(0f, 3f);
        if (ranFloat <= 1)
        {
            return 1;
        }
        else if (ranFloat <= 2 && ranFloat > 1)
        {
            return 2;
        }
        else
        {
            return 3;
        }
    }

    public void destroyPassenger()
    {
        GameManager.OnReset -= destroyPassenger;
        Destroy(this.gameObject);
    }
}
