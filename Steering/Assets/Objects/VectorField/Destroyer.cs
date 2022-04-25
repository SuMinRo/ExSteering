using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    Spawners parent;
    Stats stats;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject.GetComponent<Spawners>();
        stats = GameObject.Find("Stats").GetComponent<Stats>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Pedestrian" && collider.gameObject.GetComponent<Pedestrian>().target.ToString() + "Spawner" == gameObject.name)
        {
            stats.ReportPerformance(collider.gameObject);
            Destroy(collider.gameObject);
            parent.DecrementNumberOfPedestrians();
        }
    }
}
