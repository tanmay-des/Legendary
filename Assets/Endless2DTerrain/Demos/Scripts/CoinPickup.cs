using UnityEngine;
using System.Collections;
using Endless2DTerrain;

public class CoinPickup : MonoBehaviour {

    void OnTriggerEnter(Collider item)
    {
        
        //Assume only one terrain displayer at a time
        var terrainDisplayer = GameObject.FindObjectOfType(typeof(TerrainDisplayer)) as TerrainDisplayer;        
        if (terrainDisplayer != null && terrainDisplayer.PrefabManager != null && terrainDisplayer.PrefabManager.Pool != null)
        {
            terrainDisplayer.PrefabManager.Pool.Remove(this.gameObject);
        }
        
  
    }

}
