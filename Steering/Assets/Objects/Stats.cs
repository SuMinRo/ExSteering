using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Stats : MonoBehaviour
{
    int counter = 0;
    int walls = 0;
    int oob = 0;
    [SerializeField]
    int maxTime;
    int maxTimeIncrement;
    [SerializeField]
    TypeOfTest testType;
    Spawners spawners;
    //alpha, speed, ttca, dca
    [SerializeField]
    Vector4 sigmas;
    [SerializeField]
    Vector3 minSigmas;
    [SerializeField]
    Vector3 sigmaIncs;
    int testNumber;

    public Dictionary<int, DicEntry> oppositeSide;
    public Dictionary<int, DicEntry> nearbySide;
    Dictionary<int, Vector2> oppositeOptimumDev;
    Dictionary<int, Vector2> nearbyOptimumDev;
    Dictionary<int, Vector2> oppositeOptimumGrad;
    Dictionary<int, Vector2> nearbyOptimumGrad;

    //Just for results report
    [SerializeField]
    SteeringAlgorithm method;
    [SerializeField]
    Congestion congestion;
    [SerializeField]
    SceneType sceneType;


    void Start()
    {
        spawners = GameObject.Find("Spawners").GetComponent<Spawners>();
        if(maxTime == 0)
        {
            maxTime = int.MaxValue;
        }
        if (testType == TypeOfTest.SigmaTesting)
        {
            sigmas[0] = minSigmas[0];
            sigmas[1] = minSigmas[1];
            sigmas[2] = minSigmas[2];
        }
        maxTimeIncrement = maxTime;
        testNumber = 0;
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

        oppositeOptimumGrad[-10] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-9] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-8] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-7] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-6] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-5] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-4] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-3] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-2] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[-1] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[0] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[1] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[2] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[3] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[4] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[5] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[6] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[7] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[8] = new Vector2(32.9f, 49.4f);
        oppositeOptimumGrad[9] = new Vector2(32.9f, 49.4f);

        nearbyOptimumGrad[-10] = new Vector2(15.8f, 23.7f);
        nearbyOptimumGrad[-9] = new Vector2(16.0f, 23.9f);
        nearbyOptimumGrad[-8] = new Vector2(16.2f, 24.3f);
        nearbyOptimumGrad[-7] = new Vector2(16.6f, 24.8f);
        nearbyOptimumGrad[-6] = new Vector2(17.0f, 25.4f);
        nearbyOptimumGrad[-5] = new Vector2(17.4f, 26.1f);
        nearbyOptimumGrad[-4] = new Vector2(17.9f, 26.8f);
        nearbyOptimumGrad[-3] = new Vector2(18.4f, 27.5f);
        nearbyOptimumGrad[-2] = new Vector2(18.9f, 28.2f);
        nearbyOptimumGrad[-1] = new Vector2(19.4f, 29.0f);
        nearbyOptimumGrad[0] = new Vector2(19.9f, 29.8f);
        nearbyOptimumGrad[1] = new Vector2(20.5f, 30.6f);
        nearbyOptimumGrad[2] = new Vector2(21.0f, 31.5f);
        nearbyOptimumGrad[3] = new Vector2(21.6f, 32.3f);
        nearbyOptimumGrad[4] = new Vector2(22.2f, 33.2f);
        nearbyOptimumGrad[5] = new Vector2(22.7f, 34.0f);
        nearbyOptimumGrad[6] = new Vector2(23.3f, 34.9f);
        nearbyOptimumGrad[7] = new Vector2(23.9f, 35.8f);
        nearbyOptimumGrad[8] = new Vector2(24.5f, 36.8f);
        nearbyOptimumGrad[9] = new Vector2(25.0f, 37.4f);
    }

    void Restart()
    {
        for (int i = -10; i < 10; i++)
        {
            oppositeSide[i] = new DicEntry();
            nearbySide[i] = new DicEntry();
        }
        foreach(GameObject p in GameObject.FindGameObjectsWithTag("Pedestrian"))
            GameObject.Destroy(p);
        spawners.ResetNumberOfPedestrians();
        maxTime += maxTimeIncrement;
        testNumber++;
        if(testNumber % 100 == 0)
        {
            sigmas[2] += sigmaIncs[2];
            sigmas[1] = minSigmas[1];
            sigmas[0] = minSigmas[0];
        }
        else if(testNumber % 10 == 0)
        {
            sigmas[1] += sigmaIncs[1];
            sigmas[0] = minSigmas[0];
        }
        else
        {
            sigmas[0] += sigmaIncs[0];
        }
        counter = 0;
        walls = 0;
        oob = 0;
    }

    void Update()
    {
        if (Time.time >= maxTime || Input.GetKeyDown("x"))
        {
            //Time.timeScale = 0.0f;
            DisplayStats();
            if (testType == TypeOfTest.Regular || testNumber == 999)
                UnityEditor.EditorApplication.isPlaying = false;
            else
                Restart();
        }
    }

    public Vector4 GetSigmas() { return sigmas; }

    public void Increment()
    {
        ++counter;
        //Debug.Log(++counter + " Collisions.");
        //Time.timeScale = 0.0f;
    }

    public void WallIncrement()
    {
        ++walls;
        //Debug.Log(++walls + " Walls.");
    }

    public void OOBIncrement()
    {
        ++oob;
        //Debug.Log(++oob + " Out of Bounds");
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
        string s = ("Time: " + Time.time + "\nCollisions: " + counter + ", Walls: " + walls + ", Out of Bounds: " + oob + "\n\n");
        if (testType == TypeOfTest.SigmaTesting)
            s += ("Test Number: " + testNumber + "\nSigmas: " + sigmas + "\n\n");
        else
            s += ("Scene: " + sceneType + "\nMethod: " + method + "\nCongestion: " + congestion + "\n\n");
        for (int i = -10; i < 10; i++)
        {
            s += (string.Format("{0, 3}", i) + " Performance: " + oppositeSide[i].GetMean()[0].ToString("F8") + ", " + oppositeSide[i].GetMean()[1].ToString("F8") +  ", " + nearbySide[i].GetMean()[0].ToString("F8") + ", " + nearbySide[i].GetMean()[1].ToString("F8") + "\n");
        }
        Debug.Log(s);
        StreamWriter writer;
        if(testType == TypeOfTest.Regular)
            writer = new StreamWriter(Application.persistentDataPath +"/results.txt", true);
        else
            writer = new StreamWriter(Application.persistentDataPath + "/sigmaResults.txt", true);
        writer.WriteLine(s);
        writer.Close();
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

public enum TypeOfTest
{
    Regular,
    SigmaTesting
}

public enum Congestion
{
    Low,
    Medium,
    High
}