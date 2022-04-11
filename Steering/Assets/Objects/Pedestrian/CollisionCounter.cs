using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionCounter : MonoBehaviour
{
    [SerializeField]
    GameObject parent;
    Stats stats;
    CapsuleCollider ownCollide;
    float startTime;
    [SerializeField]
    float graceTime;
    Spawners spawners;

    // Start is called before the first frame update
    void Start()
    {
        GameObject statsObject = GameObject.Find("Stats");
        spawners = GameObject.Find("Spawners").GetComponent<Spawners>();
        stats = statsObject.GetComponent<Stats>();
        ownCollide = GetComponent<CapsuleCollider>();
        startTime = Time.time;
    }

    void Update()
    {
        //Debug.Log(startTime);
    }

    void OnTriggerEnter(Collider collide)
    {
        if (collide.gameObject.tag == "Pedestrian" && collide.gameObject != parent && Time.time > startTime + graceTime && Time.time > collide.GetComponent<Pedestrian>().startTime + graceTime && StringLessThan(parent.name, collide.gameObject.name))
        {
            stats.Increment();
            //Debug.Log("Me: " + startTime + "\nHim: " + collide.GetComponent<Pedestrian>().startTime + "\nTime: " + Time.time);
            Debug.DrawLine(parent.transform.position, parent.transform.position + new Vector3(0, 10, 0), Color.red, 0.0f, true);
        }
        else if (collide.gameObject.tag == "Obstacle")
        {
            stats.WallIncrement();
            spawners.DecrementNumberOfPedestrians();
            Destroy(parent.gameObject);
        }
    }

    public static bool StringLessThan(string compare, string to)
    {
        if (int.Parse(compare, System.Globalization.NumberStyles.HexNumber) < int.Parse(to, System.Globalization.NumberStyles.HexNumber))
            return true;
        return false;
    }
}
