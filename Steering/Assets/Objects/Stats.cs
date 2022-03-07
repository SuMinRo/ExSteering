using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stats : MonoBehaviour
{
    int counter = 0;
    int walls = 0;
    int oob = 0;
    [SerializeField]
    int maxTime;

    void Update()
    {
        if (Time.time >= maxTime)
        {
            Time.timeScale = 0.0f;
            DisplayStats();
        }
    }

    public void Increment()
    {
        Debug.Log(++counter + " Collisions.");
        //Time.timeScale = 0.0f;
    }

    public void WallIncrement()
    {
        Debug.Log(++walls + " Walls.");
    }

    public void OOBIncrement()
    {
        Debug.Log(++oob + " Out of Bounds");
    }

    public void DisplayStats()
    {
        Debug.Log("Collisions: " + counter + "\nWalls: " + walls + "\nOut of Bounds:" + oob);
    }
}
