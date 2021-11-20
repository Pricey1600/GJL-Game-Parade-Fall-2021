using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassengerSpawner : MonoBehaviour
{
    [SerializeField] private List<GameObject> passengerPrefabs;
    [SerializeField] private Transform spawnPoint;
    private int numberToSpawn;
    private Vector3 positionToSpawn;

    private void Start()
    {
        GameManager.OnReset += startSpawning;
    }
    private void OnDisable()
    {
        GameManager.OnReset -= startSpawning;
    }

    public void spawnPassenger()
    {
        GameObject prefab = passengerPrefabs[Random.Range(0, passengerPrefabs.Count)];
        Instantiate(prefab, positionToSpawn, Quaternion.identity);
    }

    private void movePositionToSpawn()
    {
        if(positionToSpawn.x == 0)
        {
            positionToSpawn = new Vector3(positionToSpawn.x + 5, transform.position.y, positionToSpawn.z);
        }
        else if(positionToSpawn.x > 0)
        {
            positionToSpawn = new Vector3(-positionToSpawn.x, transform.position.y, positionToSpawn.z);
        }
        else if (positionToSpawn.x < 0)
        {
            positionToSpawn = new Vector3(-positionToSpawn.x + 5, transform.position.y, positionToSpawn.z);
        }
    }

    public void resetPositionToSpawn()
    {
        positionToSpawn = spawnPoint.position;
    }

    public void startSpawning()
    {
        numberToSpawn = Random.Range(6, 15);
        resetPositionToSpawn();

        while(numberToSpawn > 0)
        {
            spawnPassenger();
            movePositionToSpawn();
            numberToSpawn -= 1;
        }
    }
}
