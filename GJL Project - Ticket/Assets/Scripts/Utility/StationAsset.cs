using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationAsset : MonoBehaviour
{
    private void Start()
    {
        StationSpawner.DestroyAllAssets += destroyAsset;
    }

    private void destroyAsset()
    {
        StationSpawner.DestroyAllAssets -= destroyAsset;
        Destroy(this.gameObject);
    }
}
