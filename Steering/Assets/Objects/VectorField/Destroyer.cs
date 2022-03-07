using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroyer : MonoBehaviour
{
    Spawners parent;

    // Start is called before the first frame update
    void Start()
    {
        parent = transform.parent.gameObject.GetComponent<Spawners>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == "Pedestrian" && collider.gameObject.GetComponent<Pedestrian>().target.ToString() + "Spawner" == gameObject.name)
        {
            Destroy(collider.gameObject);
            parent.DecrementNumberOfPedestrians();
        }
    }
}
