using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropPoint : MonoBehaviour
{
    public delegate void ScoreAction(int scoreChangeType);
    public static event ScoreAction OnScoreChange;

    [SerializeField] private int carriageClass = 1;
    public void ClassCheck(int ticketClass)
    {
        if(ticketClass == carriageClass)
        {
            //passenger has been placed in correct carriage
            ///Debug.Log("passenger has been placed in correct carriage");

            OnScoreChange?.Invoke(2);

        }
        else if(ticketClass != carriageClass && ticketClass != 0)
        {
            //passenger has valid ticket but has been placed in the wrong carriage
            ///Debug.Log("passenger has valid ticket but has been placed in the wrong carriage");

            OnScoreChange?.Invoke(1);
        }
        else
        {
            //passenger has no valid ticket
            ///Debug.Log("passenger has no valid ticket");

            OnScoreChange?.Invoke(0);
        }
    }
}
