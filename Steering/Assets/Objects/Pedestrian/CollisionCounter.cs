using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCounter : MonoBehaviour
{
    [SerializeField]
    GameObject parent;
    Stats stats;
    CapsuleCollider collide;
    float startTime;
    Spawners spawners;

    // Start is called before the first frame update
    void Start()
    {
        GameObject statsObject = GameObject.Find("Stats");
        spawners = GameObject.Find("Spawners").GetComponent<Spawners>();
        stats = statsObject.GetComponent<Stats>();
        collide = GetComponent<CapsuleCollider>();
        collide.enabled = false;
        startTime = Time.time;
    }

    void Update()
    {
        if(collide.enabled = false && Time.time > startTime + 1 )
            collide.enabled = true;
    }

    void OnTriggerEnter(Collider collide)
    {
        if (collide.gameObject.tag == "Pedestrian" && collide.gameObject != parent && StringLessThan(parent.name, collide.gameObject.name))
            stats.Increment();
        else if (collide.gameObject.tag == "Obstacle")
        {
            stats.WallIncrement();
            spawners.DecrementNumberOfPedestrians();
            Destroy(parent.gameObject);
        }
    }

    bool StringLessThan(string compare, string to)
    {
        if (int.Parse(compare, System.Globalization.NumberStyles.HexNumber) < int.Parse(to, System.Globalization.NumberStyles.HexNumber))
            return true;
        return false;
    }
}
