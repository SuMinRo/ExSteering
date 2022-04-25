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

    Dictionary<int, DicEntry> oppositeSide;
    Dictionary<int, DicEntry> nearbySide;
    Dictionary<int, Vector2> oppositeOptimumDev;
    Dictionary<int, Vector2> nearbyOptimumDev;
    Dictionary<int, Vector2> oppositeOptimumGrad;
    Dictionary<int, Vector2> nearbyOptimumGrad;

    void Start()
    {
        if(maxTime == 0)
        {
            maxTime = int.MaxValue;
        }
        oppositeSide = new Dictionary<int, DicEntry>();
        nearbySide = new Dictionary<int, DicEntry>();
        oppositeOptimumDev = new Dictionary<int, Vector2>();
        nearbyOptimumDev = new Dictionary<int, Vector2>();
        oppositeOptimumGrad = new Dictionary<int, Vector2>();
        nearbyOptimumGrad = new Dictionary<int, Vector2>();
        for (int i = -10; i < 10; i++)
        {
            oppositeSide[i] = new DicEntry();
            nearbySide[i] = new DicEntry();
        }
        oppositeOptimumDev[-10] = new Vector2(33.3f, 49.9f);
        oppositeOptimumDev[-9] = new Vector2(33.2f, 49.8f);
        oppositeOptimumDev[-8] = new Vector2(33.1f, 49.6f);
        oppositeOptimumDev[-7] = new Vector2(33.0f, 49.5f);
        oppositeOptimumDev[-6] = new Vector2(32.9f, 49.3f);
        oppositeOptimumDev[-5] = new Vector2(32.8f, 49.2f);
        oppositeOptimumDev[-4] = new Vector2(32.8f, 49.1f);
        oppositeOptimumDev[-3] = new Vector2(32.7f, 49.1f);
        oppositeOptimumDev[-2] = new Vector2(32.7f, 49.0f);
        oppositeOptimumDev[-1] = new Vector2(32.7f, 49.0f);
        oppositeOptimumDev[0] = new Vector2(32.7f, 49.0f);
        oppositeOptimumDev[1] = new Vector2(32.7f, 49.0f);
        oppositeOptimumDev[2] = new Vector2(32.7f, 49.1f);
        oppositeOptimumDev[3] = new Vector2(32.8f, 49.1f);
        oppositeOptimumDev[4] = new Vector2(32.8f, 49.2f);
        oppositeOptimumDev[5] = new Vector2(32.9f, 49.3f);
        oppositeOptimumDev[6] = new Vector2(33.0f, 49.5f);
        oppositeOptimumDev[7] = new Vector2(33.1f, 49.6f);
        oppositeOptimumDev[8] = new Vector2(33.2f, 49.8f);
        oppositeOptimumDev[9] = new Vector2(33.3f, 49.9f);
        
        nearbyOptimumDev[-10] = new Vector2(18.1f, 27.2f);
        nearbyOptimumDev[-9] = new Vector2(18.5f, 27.7f);
        nearbyOptimumDev[-8] = new Vector2(18.9f, 28.4f);
        nearbyOptimumDev[-7] = new Vector2(19.4f, 29.1f);
        nearbyOptimumDev[-6] = new Vector2(19.9f, 29.9f);
        nearbyOptimumDev[-5] = new Vector2(20.4f, 30.6f);
        nearbyOptimumDev[-4] = new Vector2(20.9f, 31.4f);
        nearbyOptimumDev[-3] = new Vector2(21.4f, 32.1f);
        nearbyOptimumDev[-2] = new Vector2(22.0f, 32.9f);
        nearbyOptimumDev[-1] = new Vector2(22.5f, 33.7f);
        nearbyOptimumDev[0] = new Vector2(23.0f, 34.5f);
        nearbyOptimumDev[1] = new Vector2(23.6f, 35.3f);
        nearbyOptimumDev[2] = new Vector2(24.1f, 36.1f);
        nearbyOptimumDev[3] = new Vector2(24.6f, 36.9f);
        nearbyOptimumDev[4] = new Vector2(25.2f, 37.8f);
        nearbyOptimumDev[5] = new Vector2(25.8f, 38.6f);
        nearbyOptimumDev[6] = new Vector2(26.3f, 39.5f);
        nearbyOptimumDev[7] = new Vector2(26.9f, 40.3f);
        nearbyOptimumDev[8] = new Vector2(27.4f, 41.2f);
        nearbyOptimumDev[9] = new Vector2(27.9f, 41.8f);
    }

    void Update()
    {
        if (Time.time >= maxTime || Input.GetKeyDown("s"))
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

    public void ReportPerformance(GameObject pedestrian)
    {
        Pedestrian ped = pedestrian.GetComponent<Pedestrian>();
        int index = 0;
        if((int)ped.source % 2 == 1)
        {
            index = 2;
        }
        //Debug.Log("Lifetime: " + (Time.time - ped.startTime) + ", Distance: " + ped.distanceTraveled + ", Origin: " + ped.origin[index]);
        int diff = (int)ped.source - (int)ped.target;
        if (ped.method == SteeringAlgorithm.DevRules)
        {
            if (diff % 2 == 0)
            {
                oppositeSide[Mathf.FloorToInt(ped.origin[index])].Add(oppositeOptimumDev[Mathf.FloorToInt(ped.origin[index])][0] / (Time.time - ped.startTime), oppositeOptimumDev[Mathf.FloorToInt(ped.origin[index])][1] / ped.distanceTraveled);
            }
            else if (diff % 4 == 1 || diff % 4 == -3)
            {
                if (ped.source == Cardinal.North || ped.source == Cardinal.West)
                    nearbySide[Mathf.FloorToInt(ped.origin[index])].Add(nearbyOptimumDev[Mathf.FloorToInt(ped.origin[index])][0] / (Time.time - ped.startTime), nearbyOptimumDev[Mathf.FloorToInt(ped.origin[index])][1] / ped.distanceTraveled);
                else
                    nearbySide[-Mathf.FloorToInt(ped.origin[index]) - 1].Add(nearbyOptimumDev[-Mathf.FloorToInt(ped.origin[index]) - 1][0] / (Time.time - ped.startTime), nearbyOptimumDev[-Mathf.FloorToInt(ped.origin[index]) - 1][1] / ped.distanceTraveled);
            }
            else
            {
                if (ped.source == Cardinal.North || ped.source == Cardinal.West)
                    nearbySide[-Mathf.FloorToInt(ped.origin[index]) - 1].Add(nearbyOptimumDev[-Mathf.FloorToInt(ped.origin[index]) - 1][0] / (Time.time - ped.startTime), nearbyOptimumDev[-Mathf.FloorToInt(ped.origin[index]) - 1][1] / ped.distanceTraveled);
                else
                    nearbySide[Mathf.FloorToInt(ped.origin[index])].Add(nearbyOptimumDev[Mathf.FloorToInt(ped.origin[index])][0] / (Time.time - ped.startTime), nearbyOptimumDev[Mathf.FloorToInt(ped.origin[index])][1] / ped.distanceTraveled);
            }
        }
        else
        {
            if (diff % 2 == 0)
            {
                oppositeSide[Mathf.FloorToInt(ped.origin[index])].Add(oppositeOptimumGrad[Mathf.FloorToInt(ped.origin[index])][0] / (Time.time - ped.startTime), oppositeOptimumGrad[Mathf.FloorToInt(ped.origin[index])][1] / ped.distanceTraveled);
            }
            else if (diff % 4 == 1 || diff % 4 == -3)
            {
                if (ped.source == Cardinal.North || ped.source == Cardinal.West)
                    nearbySide[Mathf.FloorToInt(ped.origin[index])].Add(nearbyOptimumGrad[Mathf.FloorToInt(ped.origin[index])][0] / (Time.time - ped.startTime), nearbyOptimumGrad[Mathf.FloorToInt(ped.origin[index])][1] / ped.distanceTraveled);
                else
                    nearbySide[-Mathf.FloorToInt(ped.origin[index]) - 1].Add(nearbyOptimumGrad[-Mathf.FloorToInt(ped.origin[index]) - 1][0] / (Time.time - ped.startTime), nearbyOptimumGrad[-Mathf.FloorToInt(ped.origin[index]) - 1][1] / ped.distanceTraveled);
            }
            else
            {
                if (ped.source == Cardinal.North || ped.source == Cardinal.West)
                    nearbySide[-Mathf.FloorToInt(ped.origin[index]) - 1].Add(nearbyOptimumGrad[-Mathf.FloorToInt(ped.origin[index]) - 1][0] / (Time.time - ped.startTime), nearbyOptimumGrad[-Mathf.FloorToInt(ped.origin[index]) - 1][1] / ped.distanceTraveled);
                else
                    nearbySide[Mathf.FloorToInt(ped.origin[index])].Add(nearbyOptimumGrad[Mathf.FloorToInt(ped.origin[index])][0] / (Time.time - ped.startTime), nearbyOptimumGrad[Mathf.FloorToInt(ped.origin[index])][1] / ped.distanceTraveled);
            }
        }
    }

    public void DisplayStats()
    {
        Debug.Log("Time: " + Time.time + "\nCollisions: " + counter + ", Walls: " + walls + ", Out of Bounds: " + oob);
        for(int i = -10; i < 10; i++)
        {
            Debug.Log("Performance: " + oppositeSide[i].GetMean() + " and " + nearbySide[i].GetMean());
        }
    }
}

public class DicEntry
{
    public int amount;
    public double totalTime;
    public double totalDistance;

    public DicEntry()
    {
        amount = 0;
        totalTime = 0.0;
        totalDistance = 0.0;
    }

    public void Add(double time, double distance)
    {
        totalTime += time;
        totalDistance += distance;
        amount++;
    }

    public Vector2 GetMean()
    {
        if(amount != 0)
            return new Vector2((float)(totalTime / amount), (float)(totalDistance / amount));
        return Vector2.zero;
    }
}