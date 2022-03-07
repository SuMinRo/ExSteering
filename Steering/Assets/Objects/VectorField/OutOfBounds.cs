using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutOfBounds : MonoBehaviour
{
    [SerializeField]
    GameObject spawnersObject;
    Spawners spawners;
    Stats stats;

    // Start is called before the first frame update
    void Start()
    {
        spawners = spawnersObject.GetComponent<Spawners>();
        stats = GameObject.Find("Stats").GetComponent<Stats>();
    }

    void OnTriggerExit(Collider collide)
    {
        if (collide.gameObject.tag == "Pedestrian")
        {
            Destroy(collide.gameObject);
            spawners.DecrementNumberOfPedestrians();
            stats.OOBIncrement();
        }
            
    }
}
