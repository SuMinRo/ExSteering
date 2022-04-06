using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class FrontDetector : MonoBehaviour
{
    [SerializeField]
    GameObject parent;
    Pedestrian pedestrian;
    Collider wall;
    private HashSet<Collider> colliders = new HashSet<Collider>();
    Vector3 closestColliderVector;
    Vector3 adjustedColliderVector;

    //For anything that is not priority only.
    List<Vector3> allVectors = new List<Vector3>();
    List<float> allWeights = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        pedestrian = parent.GetComponent<Pedestrian>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (pedestrian.strat)
        {
            case Strategy.Priority:
                UpdatePriority();
                break;
            case Strategy.PriorityDither:
                UpdatePriorityDither();
                break;
            case Strategy.Blend:
                UpdateBlend();
                break;
            case Strategy.BlendWeights:
                UpdateBlendWeights();
                break;
        }
    }

    void UpdatePriority()
    {
        closestColliderVector = Vector3.zero;
        adjustedColliderVector = Vector3.zero;
        if (wall != null)
        {
            Vector3 colliderVector = wall.ClosestPoint(parent.transform.position) - wall.transform.position;
            adjustedColliderVector = new Vector3(colliderVector[0], 0.0f, colliderVector[2]);
            pedestrian.wallDetected = true;
        }
        else
        {
            foreach (Collider collide in colliders.ToList())
            {
                if (collide == null)
                {
                    colliders.Remove(collide);
                    continue;
                }

                Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
                if (Vector3.Dot(collide.transform.forward, pedestrian.transform.forward) < 0 && (closestColliderVector.magnitude > colliderVector.magnitude || closestColliderVector == Vector3.zero))
                {
                    closestColliderVector = colliderVector;
                    pedestrian.UpdateThreat(collide, true);

                    if (Mathf.Abs(Vector3.Dot(Vector3.Normalize(closestColliderVector), collide.transform.right)) > Mathf.Sin(15 * Mathf.PI / 180))
                        adjustedColliderVector = -collide.transform.forward;
                    else
                        adjustedColliderVector = -collide.transform.right;
                }
            }

            pedestrian.wallDetected = false;
        }

        pedestrian.frontVector = adjustedColliderVector;
    }

    void UpdatePriorityDither()
    {
        allVectors.Clear();
        allWeights.Clear();
        adjustedColliderVector = Vector3.zero;
        float weightTotal = 0.0f;

        if (wall != null)
        {
            Vector3 colliderVector = wall.ClosestPoint(parent.transform.position) - wall.transform.position;
            allVectors.Add(new Vector3(colliderVector[0], 0.0f, colliderVector[2]));
            weightTotal += 1.0f / colliderVector.magnitude;
            allWeights.Add(weightTotal);
            pedestrian.wallDetected = true;
        }

        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            if (Vector3.Dot(collide.transform.forward, pedestrian.transform.forward) < 0)
            {
                weightTotal += 1.0f / colliderVector.magnitude;
                allWeights.Add(weightTotal);
                if (Mathf.Abs(Vector3.Dot(Vector3.Normalize(colliderVector), collide.transform.right)) > Mathf.Sin(15 * Mathf.PI / 180))
                    allVectors.Add(-collide.transform.forward);
                else
                    allVectors.Add(-collide.transform.right);
            }
        }

        if (allVectors.Count != 0)
        {
            float chosenVal = Random.Range(0.0f, weightTotal);
            for(int i = 0; i < allVectors.Count; i++)
            {
                if (chosenVal <= allWeights[i])
                {
                    adjustedColliderVector = allVectors[i];
                    break;
                }
            }
        }

        pedestrian.frontVector = adjustedColliderVector;
    }

    void UpdateBlend()
    {
        allVectors.Clear();
        adjustedColliderVector = Vector3.zero;

        if (wall != null)
        {
            Vector3 colliderVector = wall.ClosestPoint(parent.transform.position) - wall.transform.position;
            allVectors.Add(new Vector3(colliderVector[0], 0.0f, colliderVector[2]));
            pedestrian.wallDetected = true;
        }

        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            if (Vector3.Dot(collide.transform.forward, pedestrian.transform.forward) < 0)
            {
                if (Mathf.Abs(Vector3.Dot(Vector3.Normalize(colliderVector), collide.transform.right)) > Mathf.Sin(15 * Mathf.PI / 180))
                    allVectors.Add(-collide.transform.forward);
                else
                    allVectors.Add(-collide.transform.right);
            }
        }

        if (allVectors.Count != 0)
        {
            foreach (Vector3 v in allVectors)
            {
                adjustedColliderVector = adjustedColliderVector + v / allVectors.Count;
            }
        }

        pedestrian.frontVector = adjustedColliderVector;
    }

    void UpdateBlendWeights()
    {
        allVectors.Clear();
        allWeights.Clear();
        adjustedColliderVector = Vector3.zero;
        float weightTotal = 0.0f;

        if (wall != null)
        {
            Vector3 colliderVector = wall.ClosestPoint(parent.transform.position) - wall.transform.position;
            float weight = 1.0f / colliderVector.magnitude;
            weightTotal += weight;
            allWeights.Add(weight);
            allVectors.Add(new Vector3(colliderVector[0], 0.0f, colliderVector[2]));
            pedestrian.wallDetected = true;
        }

        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            if (Vector3.Dot(collide.transform.forward, pedestrian.transform.forward) < 0)
            {
                float weight = 1.0f / colliderVector.magnitude;
                weightTotal += weight;
                allWeights.Add(weight);
                if (Mathf.Abs(Vector3.Dot(Vector3.Normalize(colliderVector), collide.transform.right)) > Mathf.Sin(15 * Mathf.PI / 180))
                    allVectors.Add(-collide.transform.forward);
                else
                    allVectors.Add(-collide.transform.right);
            }
        }

        if (allVectors.Count != 0)
        {
            for(int i = 0; i < allVectors.Count; i++)
            {
                adjustedColliderVector = adjustedColliderVector + allVectors[i] * allWeights[i] / weightTotal;
            }
        }

        pedestrian.frontVector = adjustedColliderVector;
    }

    void OnTriggerEnter(Collider collide)
    {
        if (collide.gameObject.tag == "Obstacle")
        {
            wall = collide;
        }
        else if (collide.gameObject.tag == "Pedestrian")
        {
            colliders.Add(collide);
        }
    }

    void OnTriggerExit(Collider collide)
    {
        if (collide.gameObject.tag == "Obstacle")
        {
            wall = null;
        }
        else if (collide.gameObject.tag == "Pedestrian")
        {
            colliders.Remove(collide);
        }
    }
}
