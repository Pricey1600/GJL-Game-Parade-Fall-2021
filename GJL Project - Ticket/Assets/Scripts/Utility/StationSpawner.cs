using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationSpawner : MonoBehaviour
{
    public delegate void DestroyAction();
    public static event DestroyAction DestroyAllAssets;

    [SerializeField] List<Transform> stationAssetList;
    [SerializeField] List<Transform> stationAssetSpots;

    private void OnEnable()
    {
        GameManager.OnReset += newStation;
    }
    private void OnDisable()
    {
        GameManager.OnReset -= newStation;
    }
    public void newStation()
    {
        //call delete existing event
        DestroyAllAssets?.Invoke();
        //spawn more assets
        foreach (Transform spot in stationAssetSpots)
        {
            spawnAsset(spot);
        }
    }

    private void spawnAsset(Transform spawnSpot)
    {
        Debug.Log("Asset spawned");
        Transform chosenAsset = stationAssetList[Random.Range(0, stationAssetList.Count)];
        Instantiate(chosenAsset, spawnSpot);
    }
}
