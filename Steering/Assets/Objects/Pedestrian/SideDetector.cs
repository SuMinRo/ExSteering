using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class SideDetector : MonoBehaviour
{
    [SerializeField]
    GameObject parent;
    
    [SerializeField]
    float maxFangle;

    Pedestrian pedestrian;

    CreateSectorColliderMesh creator;

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

        creator = GameObject.Find("MeshCreator").GetComponent<CreateSectorColliderMesh>();

        GetComponent<MeshFilter>().mesh = creator.GetMesh();
        GetComponent<MeshCollider>().sharedMesh = GetComponent<MeshFilter>().mesh;
    }

    // Update is called once per frame
    void Update()
    {
        switch(pedestrian.strat)
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

        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            //Debug.Log(collide.gameObject.name +  ", Intersect: " + (SolveRayIntersect(collide) != 0.0f) + ", To the right: " + (Vector3.Dot(colliderVector, pedestrian.transform.right) > 0));
            if (SolveRayIntersect(collide) != 0.0f && Vector3.Dot(colliderVector, pedestrian.transform.right) > 0 && (closestColliderVector.magnitude > colliderVector.magnitude || closestColliderVector == Vector3.zero))
            {
                closestColliderVector = colliderVector;
                adjustedColliderVector = -collide.transform.forward;
            }

        }
        pedestrian.sideVector = adjustedColliderVector;
    }

    void UpdatePriorityDither()
    {
        allVectors.Clear();
        allWeights.Clear();
        adjustedColliderVector = Vector3.zero;
        float weightTotal = 0.0f;

        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            //Debug.Log(collide.gameObject.name +  ", Intersect: " + (SolveRayIntersect(collide) != 0.0f) + ", To the right: " + (Vector3.Dot(colliderVector, pedestrian.transform.right) > 0));
            if (SolveRayIntersect(collide) != 0.0f && Vector3.Dot(colliderVector, pedestrian.transform.right) > 0)
            {
                weightTotal += 1.0f / colliderVector.magnitude;
                allWeights.Add(weightTotal);
                allVectors.Add(-collide.transform.forward);
            }

        }

        if (allVectors.Count != 0)
        {
            float chosenVal = Random.Range(0.0f, weightTotal);
            for (int i = 0; i < allVectors.Count; i++)
            {
                if (chosenVal <= allWeights[i])
                {
                    adjustedColliderVector = allVectors[i];
                    break;
                }
            }
        }

        pedestrian.sideVector = adjustedColliderVector;
    }

    void UpdateBlend()
    {
        allVectors.Clear();
        adjustedColliderVector = Vector3.zero;;

        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            //Debug.Log(collide.gameObject.name +  ", Intersect: " + (SolveRayIntersect(collide) != 0.0f) + ", To the right: " + (Vector3.Dot(colliderVector, pedestrian.transform.right) > 0));
            if (SolveRayIntersect(collide) != 0.0f && Vector3.Dot(colliderVector, pedestrian.transform.right) > 0)
            {
                allVectors.Add(-collide.transform.forward);
            }

        }

        if (allVectors.Count != 0)
        {
            foreach (Vector3 v in allVectors)
            {
                adjustedColliderVector = adjustedColliderVector + v / allVectors.Count;
            }
        }

        pedestrian.sideVector = adjustedColliderVector;
    }

    void UpdateBlendWeights()
    {
        allVectors.Clear();
        allWeights.Clear();
        adjustedColliderVector = Vector3.zero;
        float weightTotal = 0.0f;

        foreach (Collider collide in colliders.ToList())
        {
            if (collide == null)
            {
                colliders.Remove(collide);
                continue;
            }

            Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            //Debug.Log(collide.gameObject.name +  ", Intersect: " + (SolveRayIntersect(collide) != 0.0f) + ", To the right: " + (Vector3.Dot(colliderVector, pedestrian.transform.right) > 0));
            if (SolveRayIntersect(collide) != 0.0f && Vector3.Dot(colliderVector, pedestrian.transform.right) > 0)
            {
                float weight = 1.0f / colliderVector.magnitude;
                weightTotal += weight;
                allWeights.Add(weight);
                allVectors.Add(-collide.transform.forward);
            }

        }

        if (allVectors.Count != 0)
        {
            for (int i = 0; i < allVectors.Count; i++)
            {
                adjustedColliderVector = adjustedColliderVector + allVectors[i] * allWeights[i] / weightTotal;
            }
        }

        pedestrian.sideVector = adjustedColliderVector;
    }

    void OnTriggerEnter(Collider collide)
    {   
        if (collide.gameObject != pedestrian.gameObject && collide.gameObject.tag == "Pedestrian" && gameObject.tag == "Cone")
        {
            
            //Vector3 colliderVector = collide.transform.position - pedestrian.transform.position;
            colliders.Add(collide);
            //Debug.Log(collide.gameObject.name + " Enters. Colliders registered: " + colliders.Count);
            
        }
    }

    void OnTriggerStay(Collider collide)
    {

    }

    void OnTriggerExit(Collider collide)
    {
        if (collide.gameObject != pedestrian.gameObject && collide.gameObject.tag == "Pedestrian" && gameObject.tag == "Cone")
        {

            colliders.Remove(collide);
            //float cangle = 180f * Mathf.Acos(Vector2.Dot(new Vector2(colliderVector[0], colliderVector[2]), new Vector2(pedestrian.transform.forward[0], pedestrian.transform.forward[2])) / colliderVector.magnitude) / Mathf.PI;
            //Debug.Log(collide.gameObject.name + " Leaves. Colliders registered: " + colliders.Count);
            
        }
    }

    float SolveRayIntersect(Collider collide)
    {
        Vector3 colliderPos = collide.transform.position;
        Vector3 colliderForward = collide.transform.forward;
        Vector3 parentPos = parent.transform.position;
        Vector3 parentForward = parent.transform.forward;
        //Debug.Log(colliderPos + ", " + colliderForward + ", " + parentPos + ", " + parentForward);
        float cross = colliderForward[0] * parentForward[2] - colliderForward[2] * parentForward[0];
        //float cross = parentForward[0] * colliderForward[2] - colliderForward[0];
        if (Mathf.Abs(cross) < 0.001f)
        {
            //Debug.Log("Cross " + cross);
            return 0.0f;
        }
        float u = (colliderPos[2] * parentForward[0] + parentForward[2] * parentPos[0] - parentForward[0] * parentPos[2] - colliderPos[0] * parentForward[2]) / cross;
        if (u <= 0)
        {
            //Debug.Log("u " + u);
            return 0.0f;
        }
        float v = (colliderPos[0] + colliderForward[0] * u - parentPos[0]) / parentForward[0];
        if (v <= 0 || v > creator.radius)
        {
            //Debug.Log("v " + v);
            return 0.0f;
        }
        //Debug.DrawRay(parentPos + parentForward * v, Vector3.up, Color.magenta, 0.0f, false);
        //Debug.DrawRay(colliderPos + colliderForward * u, Vector3.up, Color.magenta, 0.0f, false);
        return v;
    }
}
