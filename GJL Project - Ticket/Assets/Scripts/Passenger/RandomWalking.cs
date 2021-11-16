using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomWalking : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private float walkRadius = 1f;

    public Vector3 RandomNavmeshLocation()
    {
        Vector3 randomDirection = Random.insideUnitSphere * walkRadius;
        randomDirection += transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }

    private void Update()
    {
        if (!agent.hasPath)
        {
            agent.SetDestination(RandomNavmeshLocation());
        }
    }

    public void ToggleAgent()
    {
        if (agent.enabled == true)
        {
            agent.enabled = false;
        }
        else
        {
            agent.enabled = true;
        }
    }
}
