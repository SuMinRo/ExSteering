using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pedestrian : MonoBehaviour
{
    Cardinal source;
    [HideInInspector]
    public Cardinal target;
    public Strategy strat;
    public SteeringAlgorithm method;
    
    public float mSpeed;
    [HideInInspector]
    public float maxSpeed;
    VectorField vectorField;

    RaycastObject[] hits;
    [SerializeField]
    int raycastFidelity;

    [HideInInspector]
    public Vector3 frontVector;
    [HideInInspector]
    public bool wallDetected;
    [HideInInspector]
    public Vector3 sideVector;
    [HideInInspector]
    public Vector3 closeVector;
    Vector3 newDir;

    [HideInInspector]
    public float startTime;

    // Start is called before the first frame update
    void Start()
    {
        if(method == SteeringAlgorithm.DevRules)
        {
            GameObject plane = GameObject.Find("VectorField");
            vectorField = plane.GetComponent<VectorField>();
        }
        else
        {
            hits = new RaycastObject[raycastFidelity + 1];
            for(int i = 0; i <= raycastFidelity; i++)
            {
                hits[i] = new RaycastObject(this, i - raycastFidelity / 2);
            }
            gameObject.transform.Find("FrontDetector").gameObject.SetActive(false);
            gameObject.transform.Find("SideDetector").gameObject.SetActive(false);
            gameObject.transform.Find("CloseDetector").gameObject.SetActive(false);
        }
        
        maxSpeed = mSpeed;

        frontVector = Vector3.zero;
        startTime = Time.time;
    }

    public void SetTargetAndSource(Cardinal spawner, Cardinal goal) 
    {
        source = spawner;
        target = goal;
    }


    // For Gradient-use only.
    public Vector3 GetVelocity()
    {
        return transform.forward * mSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            //Rotate the sprite about the Y axis in the positive direction
            transform.Rotate(new Vector3(0, 20, 0) * Time.deltaTime * mSpeed, Space.World);
        }

        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            //Rotate the sprite about the Y axis in the negative direction
            transform.Rotate(new Vector3(0, -20, 0) * Time.deltaTime * mSpeed, Space.World);
        }
        //Autonomous behaviour
        else if (method == SteeringAlgorithm.DevRules)
        {
            if(frontVector.magnitude != 0.0f)
            {
                newDir = Vector3.RotateTowards(transform.forward, frontVector, Time.deltaTime, 0.0f);
            }
            else if(sideVector.magnitude != 0.0f)
            {
                newDir = Vector3.RotateTowards(transform.forward, sideVector, Time.deltaTime, 0.0f);
            }
            else
            {
                newDir = Vector3.RotateTowards(transform.forward, vectorField.Interpolate(transform.position, target), Time.deltaTime, 0.0f);
            }

            if (closeVector.magnitude > 0.0f && maxSpeed > 0.0f)
                maxSpeed -= 0.25f;
            else if (maxSpeed < mSpeed)
                maxSpeed += 0.25f;

            transform.rotation = Quaternion.LookRotation(newDir);
        }
        else
        {
            int layerMask = 1 << 0;
            foreach(RaycastObject hit in hits)
            {
                if (Physics.Raycast(transform.position, transform.TransformDirection(hit.dir * Vector3.forward), out hit.hit, 12, layerMask))
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * hit.hit.distance, Color.red);
                    hit.Perception(transform.position);
                    //Debug.Log("Hit " + hit.hit.collider.gameObject);
                }
                else
                {
                    Debug.DrawRay(transform.position, transform.TransformDirection(hit.dir * Vector3.forward) * 12, Color.green);
                    hit.Perception(Vector3.zero);
                    //Debug.Log("Did not Hit");
                }
                hit.Evaluation(transform.position, transform.forward);
                hit.Action();
            }
        }
        transform.position += transform.forward * Time.deltaTime * maxSpeed;

        if(Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (Time.timeScale == 0.0f)
                Time.timeScale = 1.0f;
            else
                Time.timeScale = 0.0f;
        }
    }
}

public enum Strategy
{
    Priority,
    PriorityDither,
    Blend,
    BlendWeights,
    None
}

public enum SteeringAlgorithm
{
    DevRules,
    Gradient
}

// For Gradient-use only.
class RaycastObject
{
    Pedestrian agent;
    Collider target;

    public Quaternion dir;
    Vector3 pos;
    Vector3 vel;
    public GameObject obj;
    public RaycastHit hit;

    float ttca;
    float dca;

    float cost;

    public RaycastObject(Pedestrian ped, int angle)
    {
        agent = ped;
        target = GameObject.Find(agent.target.ToString() + "Spawner").GetComponent<Collider>(); ;

        dir = Quaternion.Euler(0, angle, 0);
        ttca = float.MaxValue;
        dca = float.MaxValue;
        cost = float.MaxValue;
    }

    public void Perception(Vector3 ownPos)
    {
        if (ownPos != Vector3.zero)
        {
            obj = hit.collider.gameObject;
            pos = hit.point - ownPos;
            Pedestrian objScript = obj.GetComponent<Pedestrian>();
            if (objScript != null)
            {
                vel = objScript.GetVelocity();
                ttca = -Vector3.Dot(pos, vel) / Mathf.Pow(vel.magnitude, 2);
            }
            else
            {
                vel = Vector3.zero;
                ttca = 0.0f;
            }

            dca = (pos + ttca * vel).magnitude;

            //Debug.Log(ttca + "\n" + dca);
        }
        else
        {
            obj = null;
            pos = ownPos;
            ttca = float.MaxValue;
            dca = float.MaxValue;
        }
    }

    public void Evaluation(Vector3 ownPos, Vector3 ownForward)
    {
        cost = CostMovement(2.0f, 3.3f, ownPos, ownForward) + CostObstacles(1.8f, 0.3f);
    }

    float CostMovement(float alphaSigma, float speedSigma, Vector3 ownPos, Vector3 ownForward)
    {
        //Debug.DrawLine(ownPos, target.ClosestPointOnBounds(ownPos), Color.magenta);
        
        float alphaG = Mathf.Acos(Vector3.Dot(Vector3.Normalize(target.ClosestPointOnBounds(ownPos) - ownPos), ownForward)) + dir.eulerAngles[1] * Mathf.PI / 180;
        if (alphaG > Mathf.PI)
            alphaG -= 2 * Mathf.PI;

        float alphaCost = Mathf.Exp(-0.5f * Mathf.Pow(alphaG / alphaSigma, 2));
        //Debug.Log(alphaG);

        float speedCost = Mathf.Exp(-0.5f * Mathf.Pow((agent.mSpeed - agent.maxSpeed) / speedSigma, 2));
        return 1.0f - 0.5f * (alphaCost + speedCost);
    }

    float CostObstacles(float ttcaSigma, float dcaSigma)
    {
        return Mathf.Exp(-0.5f * (Mathf.Pow(ttca / ttcaSigma, 2) + Mathf.Pow(dca / dcaSigma, 2)));
    }

    public void Action()
    {

    }
}