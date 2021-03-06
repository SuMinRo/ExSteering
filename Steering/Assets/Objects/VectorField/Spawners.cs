using System.Collections;
using System.Collections.Generic;
//using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawners : MonoBehaviour
{
    [SerializeField]
    int maxNumberOfPedestrians;
    int numberOfPedestrians = 0;
    [SerializeField]
    float spawnDelay;
    [SerializeField]
    GameObject[] spawners;
    Vector2[] spawnerBBoxMin = new Vector2[4];
    Vector2[] spawnerBBoxMax = new Vector2[4];
    float[] faceDirection = new float[] { 180, -90, 0, 90};
    [SerializeField]
    GameObject pedestrian;
    int[] enums = (int[])Cardinal.GetValues(typeof(Cardinal));
    IEnumerator spawner;
    public SceneType sceneType;

    // Start is called before the first frame update
    void Start()
    {
        foreach (int dir in enums)
        {
            Vector3 bBoxMin = spawners[dir].GetComponent<BoxCollider>().bounds.min;
            spawnerBBoxMin[dir] = new Vector2(bBoxMin[0], bBoxMin[2]);
            Vector3 bBoxMax = spawners[dir].GetComponent<BoxCollider>().bounds.max;
            spawnerBBoxMax[dir] = new Vector2(bBoxMax[0], bBoxMax[2]);
        }
        spawner = SpawnCoroutine();
        StartCoroutine(spawner);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Cardinal randomCardinal = (Cardinal)enums[Random.Range(0,4)];
            Spawn(randomCardinal);
        }
    }

    IEnumerator SpawnCoroutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(spawnDelay);
            if (numberOfPedestrians < maxNumberOfPedestrians)
            {
                Cardinal randomCardinal;
                switch (sceneType)
                {
                    case SceneType.Fourway:
                        randomCardinal = (Cardinal)enums[Random.Range(0, 4)];
                        break;
                    case SceneType.Threeway:
                        randomCardinal = (Cardinal)enums[Random.Range(0, 3)];
                        break;
                    case SceneType.Twoway:
                        randomCardinal = (Cardinal)enums[Random.Range(0, 2)];
                        break;
                    default:
                        randomCardinal = (Cardinal)enums[Random.Range(0, 2) * 2];
                        break;
                }

                Spawn(randomCardinal);
            }
        }
    }

    void Spawn(Cardinal spawner)
    {
        Vector3 origin = new Vector3(Random.Range((float)spawnerBBoxMin[(int)spawner][0] + 0.5f, (float)spawnerBBoxMax[(int)spawner][0] - 0.5f), 1, Random.Range((float)spawnerBBoxMin[(int)spawner][1] + 0.5f, (float)spawnerBBoxMax[(int)spawner][1] - 0.5f));
        GameObject newPedestrian = Instantiate(pedestrian, origin, Quaternion.Euler(0, faceDirection[(int)spawner], 0));
        newPedestrian.name = Random.Range(0, 2147483647).ToString("X8");
        Pedestrian newPedestrianScript = newPedestrian.GetComponent<Pedestrian>();
        Cardinal target;
        switch (sceneType)
        {
            case SceneType.Fourway:
                target = (Cardinal)(((int)spawner + Random.Range(1, 4)) % 4);
                break;
            case SceneType.Threeway:
                target = (Cardinal)(((int)spawner + Random.Range(1, 3)) % 3);
                break;
            case SceneType.Twoway:
                target = (Cardinal)(((int)spawner + 1) % 2);
                break;
            default:
                target = (Cardinal)(((int)spawner + 2) % 4);
                break;
        }
        newPedestrianScript.SetTargetAndSource(spawner, target);
        newPedestrianScript.origin = origin;
        numberOfPedestrians++;
    }

    public void DecrementNumberOfPedestrians()
    {
        numberOfPedestrians--;
    }

    public void ResetNumberOfPedestrians()
    {
        numberOfPedestrians = 0;
    }
}

public enum Cardinal
{
    North,
    East,
    South,
    West
}

public enum SceneType
{
    Fourway,
    Threeway,
    Twoway,
    Gauntlet
}
